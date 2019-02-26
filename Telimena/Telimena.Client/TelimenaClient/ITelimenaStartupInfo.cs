using System;
using System.Collections.Generic;
using System.Reflection;
using TelimenaClient.Model;

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
        Guid TelemetryKey { get;  }

        /// <summary>
        /// OPTIONAL <para/>
        /// URL to the telemetry API. If not provided, default URI is used
        /// </summary>
        Uri TelemetryApiBaseUrl { get;  }

        /// <summary>
        /// OPTIONAL <para/>
        /// The assembly which should be treated as primary program assembly. If not provided, Telimena client will determine the assembly.
        /// </summary>
        Assembly MainAssembly { get;  }

        /// <summary>
        /// OPTIONAL <para/>
        /// Provide Program information. If not provided, Telimena client will create instance.
        /// </summary>
        ProgramInfo ProgramInfo { get;  }

        /// <summary>
        /// OPTIONAL <para/>
        /// Provide application user information. If not provided, Telimena client will create instance.
        /// </summary>
        UserInfo UserInfo { get;  }

        /// <summary>
        /// Default: TRUE <para/>
        /// If set to true Telimena will not throw any unhandled exceptions. Otherwise, errors will be thrown (for debug purpose)
        /// </summary>
        bool SuppressAllErrors { get; }

        /// <summary>
        /// Gets or sets the instrumentation key (if AppInsights is in use)
        /// </summary>
        /// <value>The instrumentation key.</value>
        string InstrumentationKey { get;  }

        /// <summary>
        /// Specify whether all unhandled exception should be tracked by telemetry
        /// </summary>
        /// <value><c>true</c> if [register unhandled exceptions tracking]; otherwise, <c>false</c>.</value>
        bool RegisterUnhandledExceptionsTracking { get;  }

        /// <summary>
        /// Gets or sets the interval between batches of telemetry being sent. Default value is 30 seconds.
        /// </summary>
        /// <value>The delivery interval.</value>
        TimeSpan DeliveryInterval { get; set; }
    }
}