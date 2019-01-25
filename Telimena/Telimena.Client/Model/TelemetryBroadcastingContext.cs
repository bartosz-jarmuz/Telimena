using System.Collections.Generic;

namespace TelimenaClient
{
    /// <summary>
    /// Class StoredTelemetryData.
    /// </summary>
    internal class TelemetryBroadcastingContext
    {
        /// <summary>
        /// Gets or sets the data pieces.
        /// </summary>
        /// <value>The files.</value>
        public List<StoredTelemetryData> Data { get; set; } = new List<StoredTelemetryData>();

        public TelemetryUpdateResponse Response { get; set; }
    }
}