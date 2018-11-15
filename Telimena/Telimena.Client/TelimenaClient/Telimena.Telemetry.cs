using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
        /// <inheritdoc />
        public Task<TelemetryUpdateResponse> ReportViewAccessedAsync(string viewName, Dictionary<string, string> telemetryData = null)
        {
            return this.Report(ApiRoutes.ReportView, viewName, telemetryData);
        }

        /// <inheritdoc />
        public TelemetryUpdateResponse ReportViewAccessedBlocking(string viewName, Dictionary<string, string> telemetryData = null)
        {
            return Task.Run(() => this.ReportViewAccessedAsync(viewName,telemetryData)).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public Task<TelemetryUpdateResponse> ReportEventAsync(string eventName, Dictionary<string, string> telemetryData = null)
        {
            return this.Report(ApiRoutes.ReportEvent, eventName, telemetryData);
        }

        /// <inheritdoc />
        public TelemetryUpdateResponse ReportEventBlocking(string eventName, Dictionary<string, string> telemetryData = null)
        {
            return Task.Run(() => this.ReportEventAsync(eventName, telemetryData)).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Where the telemetry begins...
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
                await this.InitializeIfNeeded().ConfigureAwait(false);
                request = new TelemetryUpdateRequest(this.TelemetryKey)
                {
                     UserId = this.LiveProgramInfo.UserId
                    , ComponentName = componentName
                    , AssemblyVersion = this.StaticProgramInfo.PrimaryAssembly.AssemblyVersion
                    , FileVersion = this.StaticProgramInfo.PrimaryAssembly.FileVersion
                    , TelemetryData = telemetryData
                };
                string responseContent = await this.Messenger.SendPostRequest(apiRoute, request).ConfigureAwait(false);
                return this.Serializer.Deserialize<TelemetryUpdateResponse>(responseContent);
            }
            catch (Exception ex)
            {
                TelimenaException exception = new TelimenaException($"Error occurred while sending update [{componentName}] statistics request", ex
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