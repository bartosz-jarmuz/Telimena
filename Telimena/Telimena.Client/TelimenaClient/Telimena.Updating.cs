using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelimenaClient
{
    /// <summary>
    /// Telemetry and Lifecycle Management Engine App
    /// <para>This is a client SDK that allows handling application telemetry and lifecycle</para>
    /// </summary>
    public partial class Telimena : ITelimena
    {
        /// <inheritdoc />
        public async Task<UpdateCheckResult> CheckForUpdatesAsync()
        {
            try
            {
                await this.InitializeIfNeeded().ConfigureAwait(false);

                UpdateResponse programUpdateResponse = await this.GetProgramUpdateResponse(true).ConfigureAwait(false);
                UpdateResponse updaterUpdateResponse = await this.GetUpdaterUpdateResponse(true).ConfigureAwait(false);

                return new UpdateCheckResult
                {
                    ProgramUpdatesToInstall = programUpdateResponse.UpdatePackages, UpdaterUpdate = updaterUpdateResponse?.UpdatePackages?.FirstOrDefault()
                };
            }
            catch (Exception ex)
            {
                TelimenaException exception = new TelimenaException("Error occurred while sending check for updates request", ex
                    , new KeyValuePair<Type, object>(typeof(string), this.GetUpdateRequestUrl(true)));
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
            return Task.Run(this.CheckForUpdatesAsync).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public async Task HandleUpdatesAsync(bool acceptBeta)
        {
            try
            {
                await this.InitializeIfNeeded();

                Task<UpdateResponse> programUpdateTask = this.GetProgramUpdateResponse(acceptBeta);
                Task<UpdateResponse> updaterUpdateTask = this.GetUpdaterUpdateResponse(acceptBeta);
                UpdateResponse programUpdateResponse = await programUpdateTask.ConfigureAwait(false);
                UpdateResponse updaterUpdateResponse = await updaterUpdateTask.ConfigureAwait(false);

                UpdateHandler handler = new UpdateHandler(this.Messenger, this.ProgramInfo, new DefaultWpfInputReceiver()
                    , new UpdateInstaller());
                await handler.HandleUpdates(programUpdateResponse, updaterUpdateResponse).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                TelimenaException exception = new TelimenaException("Error occurred while handling updates", ex
                    , new KeyValuePair<Type, object>(typeof(string), this.GetUpdateRequestUrl(true)));
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
        /// Gets the program update response.
        /// </summary>
        /// <param name="takeBeta">if set to <c>true</c> [take beta].</param>
        /// <returns>Task&lt;UpdateResponse&gt;.</returns>
        protected async Task<UpdateResponse> GetProgramUpdateResponse(bool takeBeta)
        {
            string responseContent = await this.Messenger.SendGetRequest(this.GetUpdateRequestUrl(takeBeta)).ConfigureAwait(false);
            return this.Serializer.Deserialize<UpdateResponse>(responseContent);
        }

        /// <summary>
        /// Gets the updater update response.
        /// </summary>
        /// <param name="takeBeta">if set to <c>true</c> [take beta].</param>
        /// <returns>Task&lt;UpdateResponse&gt;.</returns>
        protected async Task<UpdateResponse> GetUpdaterUpdateResponse(bool takeBeta)
        {
            string responseContent = await this.Messenger.SendGetRequest(this.GetUpdaterUpdateRequestUrl(takeBeta)).ConfigureAwait(false);
            return this.Serializer.Deserialize<UpdateResponse>(responseContent);
        }

        /// <summary>
        /// Gets the update request URL.
        /// </summary>
        /// <param name="takeBeta">if set to <c>true</c> [take beta].</param>
        /// <returns>System.String.</returns>
        private string GetUpdateRequestUrl(bool takeBeta)
        {
            try
            {

                UpdateRequest model = new UpdateRequest(this.ProgramId, this.ProgramVersion, this.UserId, takeBeta, this.TelimenaVersion);
                string stringified = this.Serializer.Serialize(model);
                string escaped = this.Serializer.UrlEncodeJson(stringified);
                return ApiRoutes.GetProgramUpdateInfo + "?request=" + escaped;
            }
            catch (Exception ex)
            {
                return $"Failed to get update request URL becacuse of {ex.Message}";
            }
        }

        /// <summary>
        /// Gets the updater update request URL.
        /// </summary>
        /// <param name="takeBeta">if set to <c>true</c> [take beta].</param>
        /// <returns>System.String.</returns>
        private string GetUpdaterUpdateRequestUrl(bool takeBeta)
        {
            UpdateRequest model = new UpdateRequest(this.ProgramId, this.ProgramVersion, this.UserId, takeBeta, this.TelimenaVersion, this.UpdaterVersion);
            string stringified = this.Serializer.Serialize(model);
            string escaped = this.Serializer.UrlEncodeJson(stringified);
            return ApiRoutes.GetUpdaterUpdateInfo + "?request=" + escaped;
        }
    }
}