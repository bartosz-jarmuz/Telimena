using System;

namespace TelimenaClient
{
    internal class DeliverySettings
    {
        public bool AppInsightsEndpointEnabled = false;

        public Uri TelimenaTelemetryEndpoint { get; set; }
    }
}