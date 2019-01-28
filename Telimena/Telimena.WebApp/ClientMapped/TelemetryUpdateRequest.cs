using System;
using System.Collections.Generic;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace TelimenaClient
{
    /// <summary>
    /// Class StatisticsUpdateRequest.
    /// </summary>
    public class TelemetryUpdateRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryUpdateRequest" /> class.
        /// </summary>
        [Obsolete]
        public  TelemetryUpdateRequest()
        {

        }

        /// <summary>
        /// New instance of request
        /// </summary>
        /// <param name="telemetryKey">The telemetry key.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="serializedTelemetryItems">The serialized telemetry items.</param>
        /// <exception cref="ArgumentException">Telemetry key is an empty guid. - TelemetryKey</exception>
        public TelemetryUpdateRequest(Guid telemetryKey, Guid userId, List<string> serializedTelemetryItems)
        {
            if (telemetryKey == Guid.Empty)
            {
                throw new ArgumentException("Telemetry key is an empty guid.", nameof(this.TelemetryKey));
            }

            this.TelemetryKey = telemetryKey;
            this.UserId = userId;
            this.SerializedTelemetryItems = serializedTelemetryItems;
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
        public List<string> SerializedTelemetryItems { get; set; }

    }
}