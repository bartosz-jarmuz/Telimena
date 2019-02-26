using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.ApplicationInsights.Extensibility.Implementation.Tracing;

namespace TelimenaClient
{
    /// <summary>
    /// A transmitter that will immediately send telemetry over HTTP. 
    /// Tracking items are being sent when Flush is called, or when the buffer is full (An OnFull "event" is raised) or every 30 seconds. 
    /// </summary>
    internal class TelimenaInMemoryTransmitter : InMemoryTransmitter, IDisposable
    {
        internal DeliverySettings DeliverySettings { get; }



        /// <summary>
        /// A lock object to serialize the sending calls from Flush, OnFull event and the Runner.  
        /// </summary>
        private readonly object sendingLockObj = new object();
        

        internal TelimenaInMemoryTransmitter(TelemetryBuffer buffer, DeliverySettings deliverySettings) : base(buffer)
        {
            this.DeliverySettings = deliverySettings;
            this.SendingInterval = deliverySettings.DeliveryInterval;
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
                CoreEventSource.Log.LogVerbose("No Tracking Items passed to Enqueue");
                return;
            }

            if (this.DeliverySettings == null || this.DeliverySettings.AppInsightsEndpointEnabled)
            {
                Transmission transmission = new Transmission(this.endpointAddress, data, JsonSerializer.ContentType, JsonSerializer.CompressionType, timeout);
                HttpWebResponseWrapper response = await transmission.SendAsync().ConfigureAwait(false);
                this.LogResult(response);

            }
            if (this.DeliverySettings != null)
            {
                Transmission teliTransmission = new Transmission(this.DeliverySettings.TelimenaTelemetryEndpoint, data, JsonSerializer.ContentType, JsonSerializer.CompressionType, timeout);
                HttpWebResponseWrapper response = await teliTransmission.SendAsync().ConfigureAwait(false);
                this.LogResult(response);
            }

        }

        private void LogResult(HttpWebResponseWrapper response)
        {
            if (response.StatusCode < 200 || response.StatusCode >= 300)
            {
                TelemetryDebugWriter.WriteLine($"Status code: {response.StatusCode}. Description: [{response.StatusDescription}]. {response.Content}" );
            }
        }
    }
}
