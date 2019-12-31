using System;
using System.Net.Http;
using System.Security.AccessControl;
using System.Threading.Tasks;
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
        public TelemetryModule(TelimenaProperties telimenaProperties)
        {
            this.telimenaProperties = telimenaProperties;
        }

        private readonly TelimenaProperties telimenaProperties;
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
            this.TelemetryClient = new TelemetryClientBuilder(this.telimenaProperties).GetClient();
            this.InitializeSession();
        }

        private Task<string> instrumentationKeyLoadingTask;


        private async Task<string> LoadInstrumentationKey()
        {
            //if the AI key is specified in the Telimena Portal, it should override the local one
            //however, checking if it is specified in Telimena will take time, and we are inside a constructor here - we don't want to block
            try
            {
                using (HttpClient client = new HttpClient() {BaseAddress = this.telimenaProperties.TelemetryApiBaseUrl})
                {
                    HttpResponseMessage response = await client.GetAsync(ApiRoutes.GetInstrumentationKey(this.telimenaProperties.TelemetryKey));
                    response.EnsureSuccessStatusCode();
                    string key = await response.Content.ReadAsStringAsync();
                    return key?.Trim('"');
                }
            }
            catch (Exception ex)
            {
                TelemetryDebugWriter.WriteLine($"Error while loading instrumentation key. Error: {ex}");
                return null;
            }
        }

        private void InitializeSession()
        {
            if (!isSessionStarted)
            {
                lock (SyncRoot)
                {
                    if (!isSessionStarted)
                    {
                        this.instrumentationKeyLoadingTask = this.LoadInstrumentationKey();

                        if (this.TelemetryClient != null)
                        {
                            //flushing the telemetry client is a synchronous blocking operation.
                            //As it is used during initialization, it may slow down the startup of the app
                            Task.Run(async ()=>
                            {
                                string cloudKey = await this.instrumentationKeyLoadingTask;
                                if (!string.IsNullOrEmpty(cloudKey))
                                {
                                    this.telimenaProperties.InstrumentationKey = cloudKey;
                                    this.TelemetryClient = new TelemetryClientBuilder(this.telimenaProperties).GetClient();
                                }
                                this.Event(SessionStartedEventKey);
                                this.SendAllDataNow();
                            });
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