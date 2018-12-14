using System;
using System.Reflection;

namespace TelimenaClient
{
    /// <summary>
    /// A simple data object containing the data needed for Telimena startup, and some initial settings
    /// </summary>
    public interface ITelimenaStartupInfo
    {
        /// <summary>
        /// The telemetry key for the app
        /// </summary>
        Guid TelemetryKey { get; set; }

        /// <summary>
        /// OPTIONAL <para/>
        /// URL to the telemetry API. If not provided, default URI is used
        /// </summary>
        Uri TelemetryApiBaseUrl { get; set; }

        /// <summary>
        /// OPTIONAL <para/>
        /// The assembly which should be treated as primary program assembly. If not provided, Telimena client will determine the assembly.
        /// </summary>
        Assembly MainAssembly { get; set; }

        /// <summary>
        /// OPTIONAL <para/>
        /// Provide Program information. If not provided, Telimena client will create instance.
        /// </summary>
        ProgramInfo ProgramInfo { get; set; }
    }
}