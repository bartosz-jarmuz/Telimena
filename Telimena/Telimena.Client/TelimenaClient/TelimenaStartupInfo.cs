using System;
using System.Reflection;

namespace TelimenaClient
{
    /// <summary>
    /// A simple data object containing the data needed for Telimena startup, and some initial settings
    /// </summary>
    public class TelimenaStartupInfo : ITelimenaStartupInfo
    {
        /// <summary>
        /// Creates default instance of the simple data object containing the data needed for Telimena startup, and some initial settings
        /// </summary>
        /// <param name="telemetryKey">The telemetry key for the current app</param>
        /// <param name="telemetryApiBaseUrl">The base url for the telemetry API</param>
        public TelimenaStartupInfo(Guid telemetryKey, Uri telemetryApiBaseUrl = null)
        {
            this.TelemetryKey = telemetryKey;
            this.TelemetryApiBaseUrl = telemetryApiBaseUrl;
        }
        /// <summary>
        /// The telemetry key for the app
        /// </summary>
        public Guid TelemetryKey { get; set; }

        /// <summary>
        /// OPTIONAL <para/>
        /// URL to the telemetry API. If not provided, default URI is used
        /// </summary>
        public Uri TelemetryApiBaseUrl { get; set; }


        /// <summary>
        /// OPTIONAL <para/>
        /// The assembly which should be treated as primary program assembly. If not provided, Telimena client will determine the assembly.
        /// </summary>
        public Assembly MainAssembly { get; set; }

        /// <summary>
        /// OPTIONAL <para/>
        /// Provide Program information. If not provided, Telimena client will create instance.
        /// </summary>
        public ProgramInfo ProgramInfo { get; set; }
    }
}