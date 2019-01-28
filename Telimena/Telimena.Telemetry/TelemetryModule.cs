using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using TelimenaClient;
using ITelemetryModule = TelimenaClient.ITelemetryModule;

namespace Telimena.Telemetry
{
    /// <inheritdoc />
    public class TelemetryModule : ITelemetryModule
    {
        /// <summary>
        ///     Asynchronous Telimena methods
        /// </summary>
        public TelemetryModule(ITelimena telimena)
        {
            this.telimena = telimena;
        }

        private readonly ITelimena telimena;
        private readonly ITelimenaProperties properties;

        private readonly object telemetryClientBuildingLock = new object();
        internal TelemetryClient telemetryClient;


        /// <inheritdoc />
        public void View(string viewName, Dictionary<string, object> telemetryData = null)
        {
            this.telemetryClient.TrackPageView(viewName);
        }

        public void Exception(Exception exception, Dictionary<string, object> telemetryData = null)
        {
            this.telemetryClient.TrackException(exception);
        }

        public void SendAllDataNow()
        {
            this.telemetryClient.Flush();
        }

        /// <inheritdoc />
        public void Event(string eventName, Dictionary<string, object> telemetryData = null)
        {
            //TelemetryItem item = new TelemetryItem(eventName, TelemetryItemTypes.Event, this.telimena.Properties.StaticProgramInfo.PrimaryAssembly.VersionData, telemetryData);
            this.telemetryClient.TrackEvent(eventName);

        }

        public void InitializeTelemetryClient()
        {
            lock (this.telemetryClientBuildingLock)
            {
                TelemetryConfiguration cfg = TelemetryConfiguration.Active;
                TelemetryBuffer buffer = new TelemetryBuffer();
                cfg.TelemetryChannel = new TelimenaInMemoryChannel(buffer
                    , new InMemoryTransmitter(buffer
                        , new DeliverySettings()
                        {
                            TelimenaEndpoint = new Uri(this.properties.TelemetryApiBaseUrl
                                , ApiRoutes.PostTelemetryData)
                        }));
                if (!cfg.TelemetryInitializers.Any(x => x is SequencePropertyInitializer))
                {
                    cfg.TelemetryInitializers.Add(new SequencePropertyInitializer());
                }

                var teliInitializer = cfg.TelemetryInitializers.FirstOrDefault(x => x is TelimenaPropertiesInitializer);
                if (teliInitializer != null)
                {
                    cfg.TelemetryInitializers.Remove(teliInitializer);
                }
                cfg.TelemetryInitializers.Add(new TelimenaPropertiesInitializer(this.properties));

                cfg.InstrumentationKey = "1a14064b-d326-4ce3-939e-8cba4d08c255";
                this.telemetryClient = new TelemetryClient(cfg);
            }
        }





    }
}