using System;
using System.Linq;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using TelimenaClient.Model;

namespace TelimenaClient
{
    internal class TelemetryClientBuilder
    {
        private readonly object telemetryClientBuildingLock = new object();
        private readonly ITelimenaProperties properties;

        public TelemetryClientBuilder(ITelimenaProperties properties)
        {
            this.properties = properties;
        }

       
        public TelemetryClient GetClient()
        {
            lock (this.telemetryClientBuildingLock)
            {
                TelemetryConfiguration config = TelemetryConfiguration.Active;

                this.LoadTelemetryChannel(config);

                this.LoadInitializers(config);

                var client = new TelemetryClient(config);

                this.InitializeContext(client);

                if (this.properties.InstrumentationKey != null)
                {
                    config.InstrumentationKey = this.properties.InstrumentationKey;
                    client.InstrumentationKey = this.properties.InstrumentationKey;
                }

                return client;
            }
        }

        private void InitializeContext(TelemetryClient client)
        {
            if (string.IsNullOrEmpty(client.Context.User.AccountId))
            {
                client.Context.User.AuthenticatedUserId = this.properties.UserInfo.UserIdentifier;
                client.Context.User.Id = this.properties.UserInfo.UserIdentifier;
            }
            client.Context.Device.OperatingSystem = Environment.OSVersion.ToString();
            client.Context.GlobalProperties.Add(TelimenaContextPropertyKeys.TelimenaVersion, this.properties.TelimenaVersion);
            client.Context.GlobalProperties.Add(TelimenaContextPropertyKeys.ProgramAssemblyVersion, this.properties.ProgramVersion.AssemblyVersion);
            client.Context.GlobalProperties.Add(TelimenaContextPropertyKeys.ProgramFileVersion, this.properties.ProgramVersion.FileVersion);
        }

        /// <summary>
        /// Gets the client. Swaps the channel with whatever provided
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <returns>TelemetryClient.</returns>
        [Obsolete("For tests only.")]
        internal TelemetryClient GetClient(ITelemetryChannel channel)
        {
            var client = this.GetClient();
            client.TelemetryConfiguration.TelemetryChannel = channel;
            return client;
        }

        private void LoadInitializers(TelemetryConfiguration cfg)
        {
            if (!cfg.TelemetryInitializers.Any(x => x is SequencePropertyInitializer))
            {
                cfg.TelemetryInitializers.Add(new SequencePropertyInitializer());
            }

           
        }

        private void LoadTelemetryChannel(TelemetryConfiguration cfg)
        {
            TelemetryBuffer buffer = new TelemetryBuffer();
            DeliverySettings deliverySettings = new DeliverySettings
            {
                AppInsightsEndpointEnabled = !string.IsNullOrEmpty(this.properties.InstrumentationKey)
                , TelimenaTelemetryEndpoint = GetTelimenaTelemetryEndpoint(this.properties)
                , DeliveryInterval = this.properties.StartupInfo.DeliveryInterval
            };
            TelimenaInMemoryTransmitter transmitter = new TelimenaInMemoryTransmitter(buffer, deliverySettings);

            cfg.TelemetryChannel = new TelimenaInMemoryChannel(buffer, transmitter);
        }

        internal static Uri GetTelimenaTelemetryEndpoint(ITelimenaProperties properties)
        {
            return new Uri(properties.TelemetryApiBaseUrl, ApiRoutes.PostTelemetryData + "/" + properties.TelemetryKey);

        }


    }
}