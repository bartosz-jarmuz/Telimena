using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace TelimenaClient
{
    /// <summary>
    /// Telemetry and Lifecycle Management Engine App
    /// <para>This is a client SDK that allows handling application telemetry and lifecycle</para>
    /// </summary>
    public partial class Telimena : ITelimena
    {
        /// <param name="acceptBeta"></param>
        /// <inheritdoc />
        public async Task<UpdateCheckResult> CheckForUpdatesAsync(bool acceptBeta = true)
        {
            UpdateRequest updateRequest = null;
            
            try
            {
                await this.InitializeIfNeeded().ConfigureAwait(false);
                string updaterVersion = this.GetUpdaterVersion();
                updateRequest = new UpdateRequest(this.LiveProgramInfo.ProgramId, this.ProgramVersion, this.LiveProgramInfo.UserId, acceptBeta, this.TelimenaVersion, updaterVersion);

                ConfiguredTaskAwaitable<UpdateResponse> programUpdateTask = this.GetUpdateResponse(this.GetUpdateRequestUrl(ApiRoutes.GetProgramUpdateInfo, updateRequest)).ConfigureAwait(false);
                ConfiguredTaskAwaitable<UpdateResponse> updaterUpdateTask = this.GetUpdateResponse(this.GetUpdateRequestUrl(ApiRoutes.GetUpdaterUpdateInfo, updateRequest)).ConfigureAwait(false);

                UpdateResponse programUpdateResponse = await programUpdateTask;
                UpdateResponse updaterUpdateResponse = await updaterUpdateTask;

                return new UpdateCheckResult
                {
                    ProgramUpdatesToInstall = programUpdateResponse.UpdatePackages, UpdaterUpdate = updaterUpdateResponse?.UpdatePackages?.FirstOrDefault()
                };
            }
            catch (Exception ex)
            {
                TelimenaException exception = new TelimenaException("Error occurred while sending check for updates request", ex
                    , new KeyValuePair<Type, object>(typeof(string), this.GetUpdateRequestUrl(ApiRoutes.GetProgramUpdateInfo, updateRequest))
                    , new KeyValuePair<Type, object>(typeof(string), this.GetUpdateRequestUrl(ApiRoutes.GetUpdaterUpdateInfo, updateRequest))
                    
                    );
                if (!this.SuppressAllErrors)
                {
                    throw exception;
                }

                return new UpdateCheckResult {Exception = exception};
            }
        }

        /// <inheritdoc />
        public UpdateCheckResult CheckForUpdatesBlocking()
        {
            return Task.Run(() => this.CheckForUpdatesAsync()).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public async Task HandleUpdatesAsync(bool acceptBeta)
        {
            try
            {
                UpdateCheckResult checkResult = await this.CheckForUpdatesAsync(acceptBeta);
                if (checkResult.Exception == null)
                {
                    UpdateHandler handler = new UpdateHandler(this.Messenger, this.LiveProgramInfo, new DefaultWpfInputReceiver()
                        , new UpdateInstaller(), this.Locator);
                    await handler.HandleUpdates(checkResult.ProgramUpdatesToInstall, checkResult.UpdaterUpdate).ConfigureAwait(false);
                }
                else
                {
                    throw checkResult.Exception;
                }
            }
            catch (Exception ex)
            {
                TelimenaException exception = new TelimenaException("Error occurred while handling updates", ex);
                if (!this.SuppressAllErrors)
                {
                    throw exception;
                }
            }
        }

        /// <inheritdoc />
        public void HandleUpdatesBlocking(bool acceptBeta)
        {
            Task.Run(()=> this.HandleUpdatesAsync(acceptBeta)).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets the updater update response.
        /// </summary>
        /// <returns>Task&lt;UpdateResponse&gt;.</returns>
        protected async Task<UpdateResponse> GetUpdateResponse(string requestUri)
        {
            string responseContent = await this.Messenger.SendGetRequest(requestUri).ConfigureAwait(false);
            return this.Serializer.Deserialize<UpdateResponse>(responseContent);
        }

        private string GetUpdaterVersion()
        {
            FileInfo updaterFile = this.Locator.GetUpdater();
            if (updaterFile.Exists)
            {
                FileVersionInfo version = FileVersionInfo.GetVersionInfo(updaterFile.FullName);
                return string.IsNullOrEmpty(version.FileVersion) ? "0.0.0.0" : version.FileVersion;
            }
            else
            {
                return "0.0.0.0";
            }
        }

        /// <summary>
        /// Gets the update request URL.
        /// </summary>
        /// <returns>System.String.</returns>
        private string GetUpdateRequestUrl(string baseUri, UpdateRequest model)
        {
            try
            {
                string stringifier = this.Serializer.Serialize(model);
                string escaped = this.Serializer.UrlEncodeJson(stringifier);
                return baseUri + "?request=" + escaped;
            }
            catch (Exception ex)
            {
                return $"Failed to get update request URL because of {ex.Message}";
            }
        }
    }
}