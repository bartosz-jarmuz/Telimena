using System;

namespace TelimenaClient
{
    /// <summary>
    /// Class TelemetryInitializeResponse.
    /// </summary>
    public class TelemetryInitializeResponse : TelimenaResponseBase
    {
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>The user identifier.</value>
        public Guid UserId { get; set; }
      
        /// <summary>
        /// Gets or sets the count.
        /// </summary>
        /// <value>The count.</value>
        public int Count { get; set; }
    }
}