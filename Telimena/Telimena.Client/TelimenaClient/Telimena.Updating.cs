using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Telimena.Client
{
    /// <summary>
    ///     Telemetry and Lifecycle Management Engine App
    ///     <para>This is a client SDK that allows handling application telemetry and lifecycle</para>
    /// </summary>
    public partial class Telimena : ITelimena
    {
        public string b = "a";

        private string a = "a";

        public async Task<UpdateCheckResult> CheckForUpdates()
        {
            try
            {
                UpdateResponse response = await this.GetUpdateResponse(true);
                return new UpdateCheckResult {UpdatesToInstall = response.UpdatePackages};
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

        public async Task HandleUpdates(BetaVersionSettings betaVersionSettings)
        {
            try
            {
                UpdateResponse response = await this.GetUpdateResponse(true);

                UpdateHandler handler = new UpdateHandler(this.Messenger, this.ProgramInfo, this.SuppressAllErrors, new DefaultWpfInputReceiver()
                    , new UpdateInstaller());
                await handler.HandleUpdates(response, betaVersionSettings);
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

        protected async Task<UpdateResponse> GetUpdateResponse(bool takeBeta)
        {
            await this.InitializeIfNeeded();
            string responseContent = await this.Messenger.SendGetRequest(this.GetUpdateRequestUrl(takeBeta));
            return this.Serializer.Deserialize<UpdateResponse>(responseContent);
        }

        private string GetUpdateRequestUrl(bool takeBeta)
        {
            UpdateRequest model = new UpdateRequest(this.ProgramId, this.ProgramVersion, this.UserId, takeBeta, this.TelimenaVersion);
            string stringified = this.Serializer.Serialize(model);
            string escaped = this.Serializer.UrlEncodeJson(stringified);
            return ApiRoutes.GetUpdatesInfo + "?request=" + escaped;
        }
    }
}