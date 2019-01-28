using System;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility.Implementation.Tracing;

namespace TelimenaClient.Telemetry.AppInsightsComponents
{
    /// <summary>
    /// Sends telemetry to Telimena
    /// </summary>
    /// <seealso cref="Microsoft.ApplicationInsights.Channel.Transmission" />
    internal class TelimenaInMemoryChannel : InMemoryChannel
    {
        public TelimenaInMemoryChannel(TelemetryBuffer buffer, InMemoryTransmitter transmitter) : base(buffer
            , transmitter)
        {

        }

        public override void Send(ITelemetry item)
        {
            if (this.isDisposed)
            {
                CoreEventSource.Log.InMemoryChannelSendCalledAfterBeingDisposed();
                return;
            }

            if (string.IsNullOrEmpty(item.Context.InstrumentationKey))
            {
                if (CoreEventSource.IsVerboseEnabled)
                {
                    CoreEventSource.Log.ItemMissingInstrumentationKey(item.ToString());
                }

                transmitter.DeliverySettings.AppInsightsEndpointEnabled = false;
            }
            else
            {
                transmitter.DeliverySettings.AppInsightsEndpointEnabled = true;
            }

            try
            {
                this.buffer.Enqueue(item);
            }
            catch (Exception e)
            {
                CoreEventSource.Log.LogVerbose("TelemetryBuffer.Enqueue failed: " + e.ToString());
            }

        }

    }
}