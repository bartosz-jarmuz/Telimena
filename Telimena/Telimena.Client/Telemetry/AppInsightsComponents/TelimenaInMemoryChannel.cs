using System;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility.Implementation.Tracing;

namespace TelimenaClient
{
    /// <summary>
    /// Sends telemetry to Telimena
    /// </summary>
    /// <seealso cref="Microsoft.ApplicationInsights.Channel.Transmission" />
    internal class TelimenaInMemoryChannel : InMemoryChannel
    {
        public TelimenaInMemoryChannel(TelemetryBuffer buffer, TelimenaInMemoryTransmitter transmitter) : base(buffer
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