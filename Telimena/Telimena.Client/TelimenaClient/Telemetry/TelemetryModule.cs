using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TelimenaClient
{
    /// <inheritdoc />
    public class TelemetryModule : ITelemetryModule
    {
        /// <summary>
        ///     Asynchronous Telimena methods
        /// </summary>
        public TelemetryModule(Telimena telimena)
        {
            this.telimena = telimena;
        }

        private readonly Telimena telimena;

        /// <inheritdoc />
        public void View(string viewName, Dictionary<string, object> telemetryData = null)
        {
            this.telimena.telemetryClient.TrackPageView(viewName);
        }

        /// <inheritdoc />
        public void Event(string eventName, Dictionary<string, object> telemetryData = null)
        {
            //TelemetryItem item = new TelemetryItem(eventName, TelemetryItemTypes.Event, this.telimena.Properties.StaticProgramInfo.PrimaryAssembly.VersionData, telemetryData);
            this.telimena.telemetryClient.TrackEvent(eventName);

        }

        /// <inheritdoc />
        public async Task<TelemetryInitializeResponse> Initialize(Dictionary<string, object> telemetryData = null)
        {
            TelemetryInitializeRequest request = null;
            try
            {
                request = new TelemetryInitializeRequest(this.telimena.Properties.TelemetryKey)
                {
                    ProgramInfo = this.telimena.Properties.StaticProgramInfo
                    ,
                    TelimenaVersion = this.telimena.Properties.TelimenaVersion
                    ,
                    UserInfo = this.telimena.Properties.UserInfo
                };
                TelemetryInitializeResponse response = await this.telimena.Messenger.SendPostRequest<TelemetryInitializeResponse>(ApiRoutes.Initialize, request).ConfigureAwait(false);

                await this.telimena.LoadLiveData(response).ConfigureAwait(false);

                if (response != null && response.Exception == null)
                {
                    this.telimena.IsInitialized = true;
                    this.telimena.initializationResponse = response;
                    return this.telimena.initializationResponse;
                }
                else
                {
                    return response;
                }
            }

            catch (Exception ex)
            {
                TelimenaException exception = new TelimenaException("Error occurred while sending registration request", this.telimena.Properties, ex);
                if (!this.telimena.Properties.SuppressAllErrors)
                {
                    throw exception;
                }

                return new TelemetryInitializeResponse { Exception = exception };
            }
        }



    }
}