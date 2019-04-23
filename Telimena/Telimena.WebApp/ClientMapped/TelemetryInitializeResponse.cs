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
      
    }
}