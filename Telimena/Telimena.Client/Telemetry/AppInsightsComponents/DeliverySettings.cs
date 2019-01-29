using System;

namespace TelimenaClient.Telemetry.AppInsightsComponents
{
    internal class DeliverySettings
    {
        public bool AppInsightsEndpointEnabled = false;

        public Uri TelimenaTelemetryEndpoint { get; set; }
    }
}