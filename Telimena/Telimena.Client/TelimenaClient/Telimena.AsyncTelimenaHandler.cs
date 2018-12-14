using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace TelimenaClient
{
    #region Using

    #endregion

    public partial class Telimena
    {
        /// <summary>
        ///     Asynchronous Telimena methods
        /// </summary>
        private class AsyncTelimenaHandler : IAsyncTelimenaHandler
        {
            /// <summary>
            ///     Asynchronous Telimena methods
            /// </summary>
            public AsyncTelimenaHandler(Telimena telimena)
            {
                this.telimena = telimena;
            }

            private readonly Telimena telimena;

            /// <param name="acceptBeta"></param>
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

                    string updaterVersion = this.telimena.GetUpdaterVersion();
                    updateRequest = new UpdateRequest(this.telimena.TelemetryKey, this.telimena.ProgramVersion, this.telimena.LiveProgramInfo.UserId, acceptBeta
                        , this.telimena.TelimenaVersion, updaterVersion);

                    ConfiguredTaskAwaitable<UpdateResponse> programUpdateTask = this.telimena
                        .GetUpdateResponse(this.telimena.GetUpdateRequestUrl(ApiRoutes.GetProgramUpdateInfo, updateRequest)).ConfigureAwait(false);
                    ConfiguredTaskAwaitable<UpdateResponse> updaterUpdateTask = this.telimena
                        .GetUpdateResponse(this.telimena.GetUpdateRequestUrl(ApiRoutes.GetUpdaterUpdateInfo, updateRequest)).ConfigureAwait(false);

                    UpdateResponse programUpdateResponse = await programUpdateTask;
                    UpdateResponse updaterUpdateResponse = await updaterUpdateTask;

                    return new UpdateCheckResult
                    {
                        ProgramUpdatesToInstall = programUpdateResponse.UpdatePackages
                        , UpdaterUpdate = updaterUpdateResponse?.UpdatePackages?.FirstOrDefault()
                    };
                }
                catch (Exception ex)
                {
                    TelimenaException exception = new TelimenaException("Error occurred while sending check for updates request", ex
                        , new KeyValuePair<Type, object>(typeof(string), this.telimena.GetUpdateRequestUrl(ApiRoutes.GetProgramUpdateInfo, updateRequest))
                        , new KeyValuePair<Type, object>(typeof(string), this.telimena.GetUpdateRequestUrl(ApiRoutes.GetUpdaterUpdateInfo, updateRequest)));
                    if (!this.telimena.SuppressAllErrors)
                    {
                        throw exception;
                    }

                    return new UpdateCheckResult {Exception = exception};
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
                        UpdateHandler handler = new UpdateHandler(this.telimena.Messenger, this.telimena.LiveProgramInfo, new DefaultWpfInputReceiver()
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
                    if (!this.telimena.SuppressAllErrors)
                    {
                        throw exception;
                    }

                    return new UpdateCheckResult {Exception = exception};
                }
            }

            /// <inheritdoc />
            public Task<TelemetryUpdateResponse> ReportViewAccessed(string viewName, Dictionary<string, string> telemetryData = null)
            {
                return this.telimena.Report(ApiRoutes.ReportView, viewName, telemetryData);
            }

            /// <inheritdoc />
            public Task<TelemetryUpdateResponse> ReportEvent(string eventName, Dictionary<string, string> telemetryData = null)
            {
                return this.telimena.Report(ApiRoutes.ReportEvent, eventName, telemetryData);
            }

            /// <inheritdoc />
            public async Task<TelemetryInitializeResponse> Initialize(Dictionary<string, string> telemetryData = null)
            {
                TelemetryInitializeRequest request = null;
                try
                {
                    request = new TelemetryInitializeRequest(this.telimena.TelemetryKey)
                    {
                        ProgramInfo = this.telimena.StaticProgramInfo
                        , TelimenaVersion = this.telimena.TelimenaVersion
                        , UserInfo = this.telimena.UserInfo
                        , SkipUsageIncrementation = false
                    };
                    string responseContent = await this.telimena.Messenger.SendPostRequest(ApiRoutes.Initialize, request).ConfigureAwait(false);
                    TelemetryInitializeResponse response = this.telimena.Serializer.Deserialize<TelemetryInitializeResponse>(responseContent);

                    await this.telimena.LoadLiveData(response).ConfigureAwait(false);

                    this.telimena.Locator = new Locator(this.telimena.LiveProgramInfo);


                    return response;
                }

                catch (Exception ex)
                {
                    TelimenaException exception = new TelimenaException("Error occurred while sending registration request", ex
                        , new KeyValuePair<Type, object>(typeof(TelemetryUpdateRequest), request));
                    if (!this.telimena.SuppressAllErrors)
                    {
                        throw exception;
                    }

                    return new TelemetryInitializeResponse {Exception = exception};
                }
            }
        }
    }
}