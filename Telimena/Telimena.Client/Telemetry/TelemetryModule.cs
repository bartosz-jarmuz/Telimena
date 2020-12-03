using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using TelimenaClient.Model;
using TelimenaClient.Serializer;

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
            this.userTrackingController = new UserTrackingController(this.telimenaProperties, new Locator(this.telimenaProperties.StaticProgramInfo), new TelimenaSerializer());
        }

        private readonly TelimenaProperties telimenaProperties;
        private readonly UserTrackingController userTrackingController;
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
                TelemetryDebugWriter.WriteError($"Error while loading instrumentation key. Error: {ex}");
                return null;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S2696:Instance members should not write to \"static\" fields", Justification = "<Pending>")]
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
                            Task.Run(async () =>
                            {
                                await this.userTrackingController.LoadUserInfo();
                                string cloudKey = await this.instrumentationKeyLoadingTask;
                                TelemetryClientBuilder builder = new TelemetryClientBuilder(this.telimenaProperties);
                                if (!string.IsNullOrEmpty(cloudKey))
                                {
                                    this.telimenaProperties.InstrumentationKey = cloudKey;
                                    this.TelemetryClient = builder.GetClient();
                                }

                                this.InitializeContext();
                                isSessionContextInitialized = true;
                                this.Event(SessionStartedEventKey);
                                if (this.telemetryToSendLater.Any())
                                {
                                    while (this.telemetryToSendLater.Count > 0)
                                    {
                                        ITelemetry item = this.telemetryToSendLater.Dequeue();
                                        this.TelemetryClient.Track(item);
                                    }
                                }
                                this.SendAllDataNow();
                            });
                            isSessionStarted = true;
                        }
                    }
                }
            }
        }
        
        private void InitializeContext()
        {
            if (string.IsNullOrEmpty(this.TelemetryClient.Context.User.AccountId))
            {
                this.TelemetryClient.Context.User.AuthenticatedUserId = this.telimenaProperties.UserInfo.UserIdentifier;
                this.TelemetryClient.Context.User.Id = this.telimenaProperties.UserInfo.UserIdentifier;
            }
            this.TelemetryClient.Context.Device.OperatingSystem = Environment.OSVersion.ToString();
            this.TelemetryClient.Context.GlobalProperties.Add(TelimenaContextPropertyKeys.TelimenaVersion, this.telimenaProperties.TelimenaVersion);
            this.TelemetryClient.Context.GlobalProperties.Add(TelimenaContextPropertyKeys.ProgramAssemblyVersion, this.telimenaProperties.ProgramVersion.AssemblyVersion);
            this.TelemetryClient.Context.GlobalProperties.Add(TelimenaContextPropertyKeys.ProgramFileVersion, this.telimenaProperties.ProgramVersion.FileVersion);
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
        private static bool isSessionContextInitialized;

       
    }
}