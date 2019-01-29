using System;
using System.Linq;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using TelimenaClient.Telemetry.AppInsightsComponents;

namespace TelimenaClient.Telemetry
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

                LoadTelemetryChannel(config);

                LoadInitializers(config);

                if (this.properties.InstrumentationKey != null)
                {
                    config.InstrumentationKey = this.properties.InstrumentationKey;
                }
                return new TelemetryClient(config);
            }
        }

        private void LoadInitializers(TelemetryConfiguration cfg)
        {
            if (!cfg.TelemetryInitializers.Any(x => x is SequencePropertyInitializer))
            {
                cfg.TelemetryInitializers.Add(new SequencePropertyInitializer());
            }

            ITelemetryInitializer teliInitializer =
                cfg.TelemetryInitializers.FirstOrDefault(x => x is TelimenaPropertiesInitializer);
            if (teliInitializer != null)
            {
                cfg.TelemetryInitializers.Remove(teliInitializer);
            }

            cfg.TelemetryInitializers.Add(new TelimenaPropertiesInitializer(this.properties));
        }

        private void LoadTelemetryChannel(TelemetryConfiguration cfg)
        {
            TelemetryBuffer buffer = new TelemetryBuffer();
            DeliverySettings deliverySettings = new DeliverySettings();
            deliverySettings.AppInsightsEndpointEnabled = !string.IsNullOrEmpty( this.properties.InstrumentationKey);
            deliverySettings.TelimenaTelemetryEndpoint = GetTelimenaTelemetryEndpoint(this.properties);
            TelimenaInMemoryTransmitter transmitter = new TelimenaInMemoryTransmitter(buffer, deliverySettings);

            cfg.TelemetryChannel = new TelimenaInMemoryChannel(buffer, transmitter);
        }

        internal static Uri GetTelimenaTelemetryEndpoint(ITelimenaProperties properties)
        {
            return new Uri(properties.TelemetryApiBaseUrl, ApiRoutes.PostTelemetryData + "/" + properties.TelemetryKey);

        }


    }
}