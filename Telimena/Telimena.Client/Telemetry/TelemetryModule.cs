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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="telimenaProperties"></param>
        /// <param name="userTrackingController"></param>
        internal TelemetryModule(TelimenaProperties telimenaProperties, UserTrackingController userTrackingController)
        {
            this.telimenaProperties = telimenaProperties;
            this.userTrackingController = userTrackingController;
        }

        private readonly TelimenaProperties telimenaProperties;
        private readonly UserTrackingController userTrackingController;
        

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
                //before sending the data out, we really need to wait for the settings to be read 
                //for example, if the the portal does not allow PII data sending, we don't want to go and send PII data, right?
                this.InitializeSettingsSync();

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
            this.InitializeSettings();
            this.InitializeSession();
        }

        private Task<string> instrumentationKeyLoadingTask;

        private async Task<string> LoadInstrumentationKey()
        {
            //if the AI key is specified in the Telimena Portal, it should override the local one
            //however, checking if it is specified in Telimena will take time, and we are inside a constructor here - we don't want to block
            string key = null;
            try
            {
                using (HttpClient client = new HttpClient() { BaseAddress = this.telimenaProperties.TelemetryApiBaseUrl })
                {
                    HttpResponseMessage response = await client.GetAsync(ApiRoutes.GetInstrumentationKey(this.telimenaProperties.TelemetryKey));
                    key = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                    return key?.Trim('"');
                }
            }
            catch (Exception ex)
            {
                TelemetryDebugWriter.WriteError($"Error while loading instrumentation key. Error: {ex}. Response: {key}");
                return null;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S2696:Instance members should not write to \"static\" fields", Justification = "<Pending>")]
        private void InitializeSettings()
        {
            //with each app launch we check the tracking settings in the cloud
            //however, we are in a constructor here, so we don't want to await
            //therefore, if the info has already been loaded, set the properties of this instance to what was loaded
            //so that the user info is available immediately.
            if (this.loadedUserInfo != null)
            {
                this.telimenaProperties.UserInfo = this.loadedUserInfo;
                this.InitializeContext();
            }
            if (this.loadedUserInfo == null)
            {
                if (!this.isSettingsInitialized)
                {
                    lock (SettingsSyncRoot)
                    {
                        if (!this.isSettingsInitialized && this.loadedUserInfo == null)
                        {
                            if (this.TelemetryClient != null)
                            {
                                //flushing the telemetry client is a synchronous blocking operation.
                                //As it is used during initialization, it may slow down the startup of the app
                                this.userInfoLoadingTask = Task.Run(async () =>
                                {
                                    await this.userTrackingController.LoadUserInfo();
                                    this.loadedUserInfo = this.telimenaProperties.UserInfo;
                                    this.InitializeContext();
                                    this.isSettingsInitialized = true;
                                    if (!isSessionStartedEventSent)
                                    {
                                        //    only send a session start even once, even if Teli instance is build in a program many times
                                        this.Event(BuiltInEventKeys.SessionStarted);
                                        isSessionStartedEventSent = true;
                                    }
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
                            }
                        }
                    }
                }
            }
            if (this.loadedUserInfo == null)
            {
                //if it's not been loaded, then it means that we need to go the internets
                //check the settings and then load
                //this loading MIGHT have already been initialized (in a separate thread)
                //however, before that returns, lets try setting the user info to what's stored locally
                this.telimenaProperties.UserInfo = this.userTrackingController.GetStoredUserInfo();
                //now, that still might be null or it might be different than the current settings
                //in any case, this will be overwritten once the initialization returns
            }
        }


        private void InitializeSettingsSync()
        {
            if (!this.isSettingsInitialized)
            {
                if (this.loadedUserInfo == null && !this.userInfoLoadingTask.IsCompleted)
                {
                    this.userInfoLoadingTask.GetAwaiter().GetResult();
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S2696:Instance members should not write to \"static\" fields", Justification = "<Pending>")]
        private void InitializeSession()
        {
            if (!isSessionStarted)
            {
                lock (InitializationSyncRoot)
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
                                string cloudKey = await this.instrumentationKeyLoadingTask;
                                if (!string.IsNullOrEmpty(cloudKey))
                                {
                                    TelemetryClientBuilder builder = new TelemetryClientBuilder(this.telimenaProperties);
                                    this.telimenaProperties.InstrumentationKey = cloudKey;
                                    this.TelemetryClient = builder.GetClient();
                                }
                                this.InitializeContext();
                            });
                            isSessionStarted = true;
                        }
                    }
                }
            }
        }

        private void InitializeContext()
        {
            if (this.userTrackingController.Settings.UserIdentifierMode == UserIdentifierMode.NoTelemetry)
            {
                this.telemetryDisabled = true;
            }
            if (string.IsNullOrEmpty(this.TelemetryClient.Context.User.AccountId))
            {
                this.TelemetryClient.Context.User.AuthenticatedUserId = this.telimenaProperties.UserInfo.UserIdentifier;
                this.TelemetryClient.Context.User.Id = this.telimenaProperties.UserInfo.UserIdentifier;
            }
            this.TelemetryClient.Context.Device.OperatingSystem = Environment.OSVersion.ToString();
            if (!this.TelemetryClient.Context.GlobalProperties.ContainsKey(TelimenaContextPropertyKeys.TelimenaVersion))
            {
                this.TelemetryClient.Context.GlobalProperties.Add(TelimenaContextPropertyKeys.TelimenaVersion, this.telimenaProperties.TelimenaVersion);
                this.TelemetryClient.Context.GlobalProperties.Add(TelimenaContextPropertyKeys.ProgramAssemblyVersion, this.telimenaProperties.ProgramVersion.AssemblyVersion);
                this.TelemetryClient.Context.GlobalProperties.Add(TelimenaContextPropertyKeys.ProgramFileVersion, this.telimenaProperties.ProgramVersion.FileVersion);
            }
        }



        private static readonly object InitializationSyncRoot = new object();
        private static readonly object SettingsSyncRoot = new object();

        private static bool isSessionStarted;
        private static bool isSessionStartedEventSent;
        private UserInfo loadedUserInfo;
        private bool isSettingsInitialized;
        private Task userInfoLoadingTask;
        private bool telemetryDisabled;
    }
}