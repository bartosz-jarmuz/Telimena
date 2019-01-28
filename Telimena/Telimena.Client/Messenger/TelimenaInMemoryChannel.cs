using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.ApplicationInsights.Extensibility.Implementation.Tracing;

namespace TelimenaClient
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

    public static class TelimenaContextPropertyKeys
    {
        public static string TelemetryKey { get; } = "TelemetryKey";
        public static string TelimenaVersion { get; } = "TelimenaVersion";
        public static string ProgramFileVersion { get; } = "ProgramFileVersion";
        public static string ProgramAssemblyVersion { get; } = "ProgramAssemblyVersion";

    }

    /// <summary>
    /// Adds Telimena-specific properties to each telemetry item. This class cannot be inherited.
    /// </summary>
    /// <seealso cref="Microsoft.ApplicationInsights.Extensibility.ITelemetryInitializer" />
    public sealed class TelimenaPropertiesInitializer : ITelemetryInitializer
    {
        

        private readonly ITelimenaProperties properties;

        public TelimenaPropertiesInitializer(ITelimenaProperties properties)
        {
            this.properties = properties;
        }

        /// <summary>
        /// Populates <see cref="ITelemetry.Sequence"/> with unique ID and sequential number.
        /// </summary>
        public void Initialize(ITelemetry telemetry)
        {
            if (!telemetry.Context.GlobalProperties.ContainsKey(TelimenaContextPropertyKeys.TelemetryKey))
            {
                telemetry.Context.GlobalProperties.Add(TelimenaContextPropertyKeys.TelemetryKey, this.properties.TelemetryKey.ToString());
            }
            if (string.IsNullOrEmpty(telemetry.Context.User.AccountId))
            {
                telemetry.Context.User.AccountId = this.properties.UserInfo.UserName;
            }
            telemetry.Context.GlobalProperties.Add(TelimenaContextPropertyKeys.TelimenaVersion, this.properties.TelimenaVersion);
            telemetry.Context.GlobalProperties.Add(TelimenaContextPropertyKeys.ProgramAssemblyVersion, this.properties.ProgramVersion.AssemblyVersion);
            telemetry.Context.GlobalProperties.Add(TelimenaContextPropertyKeys.ProgramFileVersion, this.properties.ProgramVersion.FileVersion);
        }
    }
}