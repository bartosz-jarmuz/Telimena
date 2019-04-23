using System;
using System.Collections.Generic;

namespace TelimenaClient.Model
{
    /// <summary>
    /// Class TelemetryInitializeRequest.
    /// </summary>
    public class TelemetryInitializeRequest
    {
        /// <summary>
        /// For serialization
        /// </summary>
        [Obsolete]
        public TelemetryInitializeRequest()
        {

        }
        /// <summary>
        /// New instance of request
        /// </summary>
        /// <param name="telemetryKey"></param>
        public TelemetryInitializeRequest(Guid telemetryKey)
        {
            if (telemetryKey == Guid.Empty)
            {
                throw new ArgumentException("Tracking key is an empty guid.",nameof(this.TelemetryKey));
            }
            this.TelemetryKey = telemetryKey;
        }

        /// <summary>
        /// Gets or sets the unique program's telemetry key
        /// </summary>
        /// <value>The program identifier.</value>
        public Guid TelemetryKey { get; set; } 

        /// <summary>
        /// Gets or sets the user information.
        /// </summary>
        /// <value>The user information.</value>
        public UserInfo UserInfo { get; set; }
        /// <summary>
        /// Gets or sets the program information.
        /// </summary>
        /// <value>The program information.</value>
        public ProgramInfo ProgramInfo { get; set; }
        /// <summary>
        /// Gets or sets the telimena version.
        /// </summary>
        /// <value>The telimena version.</value>
        public string TelimenaVersion { get; set; }
      
        /// <summary>
        /// Gets or sets the custom data.
        /// </summary>
        /// <value>The custom data.</value>
        public Dictionary<string, string> TelemetryData { get; set; }
    }
}