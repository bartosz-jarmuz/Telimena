using System;
using System.Security.AccessControl;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;

namespace TelimenaClient
{
    /// <inheritdoc />
    public partial class TelemetryModule : ITelemetryModule
    {
        /// <summary>
        ///     Asynchronous Telimena methods
        /// </summary>
        public TelemetryModule(ITelimenaProperties telimenaProperties)
        {
            this.telimenaProperties = telimenaProperties;
        }

        private readonly ITelimenaProperties telimenaProperties;
        private static readonly string SessionStartedEventKey = "TelimenaSessionStarted";

        /// <summary>
        /// Gets the telemetry client.
        /// </summary>
        /// <value>The telemetry client.</value>
        public TelemetryClient TelemetryClient { get; private set; }

        /// <inheritdoc />

        public void SendAllDataNow()
        {
            try
            {
                this.TelemetryClient.Flush();
            }
            catch (Exception)
            {
                if (!this.telimenaProperties.SuppressAllErrors)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Initializes the telemetry client.
        /// </summary>
        public void InitializeTelemetryClient()
        {
            TelemetryClientBuilder builder = new TelemetryClientBuilder(this.telimenaProperties);
            this.TelemetryClient = builder.GetClient();
            this.InitializeSession();
        }

        private void InitializeSession()
        {
            if (!isSessionStarted)
            {
                lock (SyncRoot)
                {
                    if (!isSessionStarted)
                    {
                        if (this.TelemetryClient != null)
                        {
                            this.Event(SessionStartedEventKey);
                            this.SendAllDataNow();
                            isSessionStarted = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Initializes the telemetry client.
        /// </summary>
        [Obsolete("For tests only")]
        internal void InitializeTelemetryClient(ITelemetryChannel channel)
        {
            TelemetryClientBuilder builder = new TelemetryClientBuilder(this.telimenaProperties);
#pragma warning disable 618
            this.TelemetryClient = builder.GetClient(channel);
#pragma warning restore 618
            this.InitializeSession();
        }

        private static readonly object SyncRoot = new object();

        private static bool isSessionStarted;
        


    }
}