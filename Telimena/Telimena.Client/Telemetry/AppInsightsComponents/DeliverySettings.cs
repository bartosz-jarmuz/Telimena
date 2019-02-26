using System;

namespace TelimenaClient
{
    internal class DeliverySettings
    {
        public bool IsHeartbeatEnabled { get; set; } = false;

        public bool AppInsightsEndpointEnabled { get; set; }= false;

        public Uri TelimenaTelemetryEndpoint { get; set; }

        public TimeSpan DeliveryInterval { get; set; }
    }
}