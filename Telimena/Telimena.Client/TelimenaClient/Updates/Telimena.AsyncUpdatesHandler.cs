using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace TelimenaClient
{
    #region Using

    #endregion

    public partial class Telimena
    {
        internal class AsyncUpdatesHandler : IAsyncUpdatesHandler
        {
            /// <summary>
            ///     Asynchronous Telimena methods
            /// </summary>
            public AsyncUpdatesHandler(Telimena telimena)
            {
                this.telimena = telimena;
            }

            private readonly Telimena telimena;
            
            /// <inheritdoc />
            public async Task<UpdateCheckResult> CheckForUpdates(bool acceptBeta = true)
            {
                UpdateRequest updateRequest = null;

                try
                {
                    TelemetryInitializeResponse result = await this.telimena.InitializeIfNeeded().ConfigureAwait(false);
                    if (result.Exception != null)
                    {
                        throw result.Exception;
                    }

                    string updaterVersion = this.GetUpdaterVersion();
                    updateRequest = new UpdateRequest(this.telimena.Properties.TelemetryKey, this.telimena.Properties.ProgramVersion, this.telimena.Properties.LiveProgramInfo.UserId, acceptBeta
                        , this.telimena.Properties.TelimenaVersion, updaterVersion);

                    ConfiguredTaskAwaitable<UpdateResponse> programUpdateTask = this
                        .GetUpdateResponse(this.GetUpdateRequestUrl(ApiRoutes.GetProgramUpdateInfo, updateRequest)).ConfigureAwait(false);
                    ConfiguredTaskAwaitable<UpdateResponse> updaterUpdateTask = this
                        .GetUpdateResponse(this.GetUpdateRequestUrl(ApiRoutes.GetUpdaterUpdateInfo, updateRequest)).ConfigureAwait(false);

                    UpdateResponse programUpdateResponse = await programUpdateTask;
                    UpdateResponse updaterUpdateResponse = await updaterUpdateTask;

                    return new UpdateCheckResult
                    {
                        ProgramUpdatesToInstall = programUpdateResponse.UpdatePackages
                        ,
                        UpdaterUpdate = updaterUpdateResponse?.UpdatePackages?.FirstOrDefault()
                    };
                }
                catch (Exception ex)
                {
                    TelimenaException exception = new TelimenaException("Error occurred while sending check for updates request", ex
                        , new KeyValuePair<Type, object>(typeof(string), this.GetUpdateRequestUrl(ApiRoutes.GetProgramUpdateInfo, updateRequest))
                        , new KeyValuePair<Type, object>(typeof(string), this.GetUpdateRequestUrl(ApiRoutes.GetUpdaterUpdateInfo, updateRequest)));
                    if (!this.telimena.Properties.SuppressAllErrors)
                    {
                        throw exception;
                    }

                    return new UpdateCheckResult { Exception = exception };
                }
            }

            /// <inheritdoc />
            public async Task<UpdateCheckResult> HandleUpdates(bool acceptBeta)
            {
                try
                {
                    UpdateCheckResult checkResult = await this.CheckForUpdates(acceptBeta);
                    if (checkResult.Exception == null)
                    {
                        UpdateHandler handler = new UpdateHandler(this.telimena.Messenger, this.telimena.Properties.LiveProgramInfo, new DefaultWpfInputReceiver()
                            , new UpdateInstaller(), this.telimena.Locator);
                        await handler.HandleUpdates(checkResult.ProgramUpdatesToInstall, checkResult.UpdaterUpdate).ConfigureAwait(false);
                    }
                    else
                    {
                        throw checkResult.Exception;
                    }

                    return checkResult;
                }
                catch (Exception ex)
                {
                    TelimenaException exception = new TelimenaException("Error occurred while handling updates", ex);
                    if (!this.telimena.Properties.SuppressAllErrors)
                    {
                        throw exception;
                    }

                    return new UpdateCheckResult { Exception = exception };
                }
            }

            /// <summary>
            ///     Gets the update request URL.
            /// </summary>
            /// <returns>System.String.</returns>
            private string GetUpdateRequestUrl(string baseUri, UpdateRequest model)
            {
                try
                {
                    string stringifier = this.telimena.Serializer.Serialize(model);
                    string escaped = this.telimena.Serializer.UrlEncodeJson(stringifier);
                    return baseUri + "?request=" + escaped;
                }
                catch (Exception ex)
                {
                    return $"Failed to get update request URL because of {ex.Message}";
                }
            }

            /// <summary>
            ///     Gets the updater update response.
            /// </summary>
            /// <returns>Task&lt;UpdateResponse&gt;.</returns>
            private async Task<UpdateResponse> GetUpdateResponse(string requestUri)
            {
                string responseContent = await this.telimena.Messenger.SendGetRequest(requestUri).ConfigureAwait(false);
                return this.telimena.Serializer.Deserialize<UpdateResponse>(responseContent);
            }

            internal string GetUpdaterVersion()
            {
                FileInfo updaterFile = this.telimena.Locator.GetUpdater();
                if (updaterFile.Exists)
                {
                    string version = TelimenaVersionReader.ReadToolkitVersion(updaterFile.FullName);
                    return string.IsNullOrEmpty(version) ? "0.0.0.0" : version;
                }

                return "0.0.0.0";
            }
        }
    }
}