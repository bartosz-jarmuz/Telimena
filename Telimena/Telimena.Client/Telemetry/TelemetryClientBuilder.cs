using System;
using System.Linq;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
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
            try
            {

                lock (this.telemetryClientBuildingLock)
                {
                    TelemetryConfiguration config = TelemetryConfiguration.Active;
                   
                    this.LoadTelemetryChannel(config);

                    this.LoadInitializers(config);

                    var client = new TelemetryClient(config);

                    client.TelemetryKey = this.properties.TelemetryKey;

                    if (this.properties.InstrumentationKey != null)
                    {
                        config.InstrumentationKey = this.properties.InstrumentationKey;
                        client.InstrumentationKey = this.properties.InstrumentationKey;
                    }
                    return client;
                }
            }
            catch(Exception e)
            {
                TelemetryDebugWriter.WriteLine($"Failed to load {nameof(TelemetryClient)}. Error: {e}");
                return null;
            }
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
                , TelimenaTelemetryKey = this.properties.TelemetryKey
            };
            TelimenaInMemoryTransmitter transmitter = new TelimenaInMemoryTransmitter(buffer, deliverySettings);

            cfg.TelemetryChannel = new TelimenaInMemoryChannel(buffer, transmitter);
        }

        internal static Uri GetTelimenaTelemetryEndpoint(ITelimenaProperties properties)
        {
            return new Uri(properties.TelemetryApiBaseUrl, ApiRoutes.PostTelemetryData);

        }


    }
}