using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.ApplicationInsights.Extensibility.Implementation.Tracing;

namespace TelimenaClient.Telemetry.AppInsightsComponents
{
    /// <summary>
    /// A transmitter that will immediately send telemetry over HTTP. 
    /// Telemetry items are being sent when Flush is called, or when the buffer is full (An OnFull "event" is raised) or every 30 seconds. 
    /// </summary>
    internal class TelimenaInMemoryTransmitter : InMemoryTransmitter, IDisposable
    {
        private readonly TelemetryBuffer buffer;
        internal TelimenaClient.Telemetry.AppInsightsComponents.DeliverySettings DeliverySettings { get; }

        /// <summary>
        /// A lock object to serialize the sending calls from Flush, OnFull event and the Runner.  
        /// </summary>
        private readonly object sendingLockObj = new object();
        
        /// <summary>
        /// The number of times this object was disposed.
        /// </summary>
        private int disposeCount = 0;

        internal TelimenaInMemoryTransmitter(TelemetryBuffer buffer, DeliverySettings deliverySettings) : base(buffer)
        {
            this.DeliverySettings = deliverySettings;
        }


        /// <summary>
        /// Serializes a list of telemetry items and sends them.
        /// </summary>
        protected override  async Task Send(IEnumerable<ITelemetry> telemetryItems, TimeSpan timeout)
        {
            byte[] data = null;

            if (telemetryItems != null)
            {
                data = JsonSerializer.Serialize(telemetryItems);
            }

            if (data == null || data.Length == 0)
            {
                CoreEventSource.Log.LogVerbose("No Telemetry Items passed to Enqueue");
                return;
            }

            if (this.DeliverySettings == null || this.DeliverySettings.AppInsightsEndpointEnabled)
            {
                var transmission = new Transmission(this.endpointAddress, data, JsonSerializer.ContentType, JsonSerializer.CompressionType, timeout);
                await transmission.SendAsync().ConfigureAwait(false);
            }
            if (this.DeliverySettings != null)
            {
                var teliTransmission = new Transmission(this.DeliverySettings.TelimenaTelemetryEndpoint, data, JsonSerializer.ContentType, JsonSerializer.CompressionType, timeout);
                await teliTransmission.SendAsync().ConfigureAwait(false);
            }

        }
    }
}
