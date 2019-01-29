using System;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace TelimenaClient.Telemetry.AppInsightsComponents
{
    /// <summary>
    /// Adds Telimena-specific properties to each telemetry item. This class cannot be inherited.
    /// </summary>
    /// <seealso cref="Microsoft.ApplicationInsights.Extensibility.ITelemetryInitializer" />
    public sealed class TelimenaPropertiesInitializer : ITelemetryInitializer
    {
        private readonly ITelimenaProperties properties;
        /// <summary>
        /// Initializes a new instance of the <see cref="TelimenaPropertiesInitializer"/> class.
        /// </summary>
        /// <param name="properties">The properties.</param>
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
                telemetry.Context.User.AuthenticatedUserId = this.properties.UserInfo.UserName;
                telemetry.Context.User.Id = this.properties.UserInfo.UserName;
            }
            telemetry.Context.Device.OperatingSystem = Environment.OSVersion.ToString();
            telemetry.Context.GlobalProperties.Add(TelimenaContextPropertyKeys.TelimenaVersion, this.properties.TelimenaVersion);
            telemetry.Context.GlobalProperties.Add(TelimenaContextPropertyKeys.ProgramAssemblyVersion, this.properties.ProgramVersion.AssemblyVersion);
            telemetry.Context.GlobalProperties.Add(TelimenaContextPropertyKeys.ProgramFileVersion, this.properties.ProgramVersion.FileVersion);
        }
    }
}