using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Telimena.Client
{
    /// <summary>
    ///     Telemetry and Lifecycle Management Engine App
    ///     <para>This is a client SDK that allows handling application telemetry and lifecycle</para>
    /// </summary>
    public partial class Telimena : ITelimena
    {
        public async Task<UpdateCheckResult> CheckForUpdates()
        {
            try
            {
                UpdateResponse programUpdateResponse = await this.GetProgramUpdateResponse(true);
                UpdateResponse updaterUpdateResponse = await this.GetUpdaterUpdateResponse(true);

                return new UpdateCheckResult
                {
                    ProgramUpdatesToInstall = programUpdateResponse.UpdatePackages
                    ,UpdaterUpdate = updaterUpdateResponse?.UpdatePackages?.FirstOrDefault()
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

                return new UpdateCheckResult {Error = exception};
            }
        }

        

        public async Task HandleUpdates(bool acceptBeta)
        {
            try
            {
                var programUpdateTask = this.GetProgramUpdateResponse(acceptBeta);
                var updaterUpdateTask = this.GetUpdaterUpdateResponse(acceptBeta);
                UpdateResponse programUpdateResponse = await programUpdateTask;
                UpdateResponse updaterUpdateResponse = await updaterUpdateTask;

                UpdateHandler handler = new UpdateHandler(this.Messenger, this.ProgramInfo, this.SuppressAllErrors, new DefaultWpfInputReceiver()
                    , new UpdateInstaller());
                await handler.HandleUpdates(programUpdateResponse, updaterUpdateResponse);
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

        protected async Task<UpdateResponse> GetProgramUpdateResponse(bool takeBeta)
        {
            await this.InitializeIfNeeded();
            string responseContent = await this.Messenger.SendGetRequest(this.GetUpdateRequestUrl(takeBeta));
            return this.Serializer.Deserialize<UpdateResponse>(responseContent);
        }

        protected async Task<UpdateResponse> GetUpdaterUpdateResponse(bool takeBeta)
        {
            await this.InitializeIfNeeded();
            string responseContent = await this.Messenger.SendGetRequest(this.GetUpdaterUpdateRequestUrl(takeBeta));
            return this.Serializer.Deserialize<UpdateResponse>(responseContent);
        }

        private string GetUpdateRequestUrl(bool takeBeta)
        {
            UpdateRequest model = new UpdateRequest(this.ProgramId, this.ProgramVersion, this.UserId, takeBeta, this.TelimenaVersion);
            string stringified = this.Serializer.Serialize(model);
            string escaped = this.Serializer.UrlEncodeJson(stringified);
            return ApiRoutes.GetProgramUpdateInfo + "?request=" + escaped;
        }

        private string GetUpdaterUpdateRequestUrl(bool takeBeta)
        {
            UpdateRequest model = new UpdateRequest(this.ProgramId, this.ProgramVersion, this.UserId, takeBeta, this.TelimenaVersion, this.UpdaterVersion);
            string stringified = this.Serializer.Serialize(model);
            string escaped = this.Serializer.UrlEncodeJson(stringified);
            return ApiRoutes.GetUpdaterUpdateInfo + "?request=" + escaped;
        }
    }
}