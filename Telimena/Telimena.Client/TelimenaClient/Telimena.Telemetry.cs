using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TelimenaClient
{
    #region Using

    #endregion

    /// <summary>
    ///     Telemetry and Lifecycle Management Engine App
    ///     <para>This is a client SDK that allows handling application telemetry and lifecycle</para>
    /// </summary>
    public partial class Telimena : ITelimena
    {
        /// <summary>
        ///     Where the telemetry begins...
        /// </summary>
        /// <param name="apiRoute"></param>
        /// <param name="componentName"></param>
        /// <param name="telemetryData"></param>
        /// <returns></returns>
        private async Task<TelemetryUpdateResponse> Report(string apiRoute, string componentName, Dictionary<string, string> telemetryData = null)
        {
            TelemetryUpdateRequest request = null;
            try
            {
                TelemetryInitializeResponse result = await this.InitializeIfNeeded().ConfigureAwait(false);
                if (result.Exception != null)
                {
                    throw result.Exception;
                }

                request = new TelemetryUpdateRequest(this.TelemetryKey)
                {
                    UserId = this.LiveProgramInfo.UserId
                    , ComponentName = componentName
                    , VersionData = this.StaticProgramInfo.PrimaryAssembly.VersionData
                    , TelemetryData = telemetryData
                };
                string responseContent = await this.Messenger.SendPostRequest(apiRoute, request).ConfigureAwait(false);
                return this.Serializer.Deserialize<TelemetryUpdateResponse>(responseContent);
            }
            catch (Exception ex)
            {
                TelimenaException exception = new TelimenaException($"Error occurred while sending update [{componentName}] telemetry request", ex
                    , new KeyValuePair<Type, object>(typeof(TelemetryUpdateRequest), request));
                if (!this.SuppressAllErrors)
                {
                    throw exception;
                }

                return new TelemetryUpdateResponse {Exception = exception};
            }
        }
    }
}