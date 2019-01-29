using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights;

namespace TelimenaClient.Telemetry
{
    /// <inheritdoc />
    public class TelemetryModule : ITelemetryModule
    {
        /// <summary>
        ///     Asynchronous Telimena methods
        /// </summary>
        public TelemetryModule(ITelimena telimena, ITelimenaProperties properties)
        {
            this.telimena = telimena;
            this.properties = properties;
        }

        private readonly ITelimena telimena;
        private readonly ITelimenaProperties properties;

        private TelemetryClient telemetryClient;


        /// <inheritdoc />
        public void View(string viewName, Dictionary<string, object> telemetryData = null)
        {
            this.telemetryClient.TrackPageView(viewName);
        }

        /// <inheritdoc />

        public void Exception(Exception exception, Dictionary<string, object> telemetryData = null)
        {
            this.telemetryClient.TrackException(exception);
        }

        /// <inheritdoc />

        public void SendAllDataNow()
        {
            this.telemetryClient.Flush();
        }

        /// <summary>
        /// Initializes the telemetry client.
        /// </summary>
        public void InitializeTelemetryClient()
        {
            TelemetryClientBuilder builder = new TelemetryClientBuilder(this.properties);
            this.telemetryClient = builder.GetClient();
        }

        /// <inheritdoc />
        public void Event(string eventName, Dictionary<string, object> telemetryData = null)
        {
            this.telemetryClient.TrackEvent(eventName);

        }

     
    }
}