using System;
using System.Collections.Generic;

namespace TelimenaClient
{
    /// <summary>
    /// Class StatisticsUpdateRequest.
    /// </summary>
    public class TelemetryUpdateRequest
    {
        [Obsolete]
        /// <summary>
        /// For serialization
        /// </summary>
        public  TelemetryUpdateRequest()
        {

        }


        /// <summary>
        /// New instance of request
        /// </summary>
        /// <param name="telemetryKey"></param>
        public TelemetryUpdateRequest(Guid telemetryKey)
        {
            if (telemetryKey == Guid.Empty)
            {
                throw new ArgumentException("Telemetry key is an empty guid.", nameof(this.TelemetryKey));
            }

            this.TelemetryKey = telemetryKey;
        }
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>The user identifier.</value>
        public Guid UserId { get; set; }
        /// <summary>
        /// Gets or sets the unique program's telemetry key
        /// </summary>
        /// <value>The program identifier.</value>
        public Guid TelemetryKey { get; set; }
      
        /// <summary>
        /// The actual telemetry data
        /// </summary>
        public List<string> SerializedTelemetryUnits { get; set; }

        /// <summary>
        /// Requests sent in debug mode will return telemetry data response
        /// </summary>
        public bool DebugMode { get; set; }
    }
}