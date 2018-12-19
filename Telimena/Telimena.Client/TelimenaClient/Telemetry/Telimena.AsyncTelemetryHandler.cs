using System;
using System.Collections.Generic;
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
        internal class AsyncTelemetryHandler : IAsyncTelemetryHandler
        {
            /// <summary>
            ///     Asynchronous Telimena methods
            /// </summary>
            public AsyncTelemetryHandler(Telimena telimena)
            {
                this.telimena = telimena;
            }

            private readonly Telimena telimena;

            /// <inheritdoc />
            public Task<TelemetryUpdateResponse> View(string viewName, Dictionary<string, string> telemetryData = null)
            {
                return this.Report(ApiRoutes.ReportView, viewName, telemetryData);
            }

            /// <inheritdoc />
            public Task<TelemetryUpdateResponse> Event(string eventName, Dictionary<string, string> telemetryData = null)
            {
                return this.Report(ApiRoutes.ReportEvent, eventName, telemetryData);
            }

            /// <inheritdoc />
            public async Task<TelemetryInitializeResponse> Initialize(Dictionary<string, string> telemetryData = null)
            {
                TelemetryInitializeRequest request = null;
                try
                {
                    request = new TelemetryInitializeRequest(this.telimena.Properties.TelemetryKey)
                    {
                        ProgramInfo = this.telimena.Properties.StaticProgramInfo
                        , TelimenaVersion = this.telimena.Properties.TelimenaVersion
                        , UserInfo = this.telimena.Properties.UserInfo
                        , SkipUsageIncrementation = false
                    };
                    string responseContent = await this.telimena.Messenger.SendPostRequest(ApiRoutes.Initialize, request).ConfigureAwait(false);
                    TelemetryInitializeResponse response = this.telimena.Serializer.Deserialize<TelemetryInitializeResponse>(responseContent);

                    await this.telimena.LoadLiveData(response).ConfigureAwait(false);

                    this.telimena.Locator = new Locator(this.telimena.Properties.LiveProgramInfo);


                    return response;
                }

                catch (Exception ex)
                {
                    TelimenaException exception = new TelimenaException("Error occurred while sending registration request", this.telimena.Properties, ex
                        , new KeyValuePair<Type, object>(typeof(TelemetryUpdateRequest), request));
                    if (!this.telimena.Properties.SuppressAllErrors)
                    {
                        throw exception;
                    }

                    return new TelemetryInitializeResponse {Exception = exception};
                }
            }

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
                    TelemetryInitializeResponse result = await this.telimena.InitializeIfNeeded().ConfigureAwait(false);
                    if (result.Exception != null)
                    {
                        throw result.Exception;
                    }

                    request = new TelemetryUpdateRequest(this.telimena.Properties.TelemetryKey)
                    {
                        UserId = this.telimena.Properties.LiveProgramInfo.UserId,
                        ComponentName = componentName,
                        VersionData = this.telimena.Properties.StaticProgramInfo.PrimaryAssembly.VersionData,
                        TelemetryData = telemetryData
                    };
                    string responseContent = await this.telimena.Messenger.SendPostRequest(apiRoute, request).ConfigureAwait(false);
                    return this.telimena.Serializer.Deserialize<TelemetryUpdateResponse>(responseContent);
                }
                catch (Exception ex)
                {
                    TelimenaException exception = new TelimenaException($"Error occurred while sending update [{componentName}] telemetry request to [{apiRoute}]", this.telimena.Properties, ex
                        , new KeyValuePair<Type, object>(typeof(TelemetryUpdateRequest), request));
                    if (!this.telimena.Properties.SuppressAllErrors)
                    {
                        throw exception;
                    }

                    return new TelemetryUpdateResponse { Exception = exception };
                }
            }
        }
    }
}