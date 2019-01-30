using System;

namespace TelimenaClient
{
    /// <summary>
    /// Class TelemetryInitializeResponse.
    /// </summary>
    public class TelemetryInitializeResponse 
    {
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>The user identifier.</value>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        /// <value>The exception.</value>
        public Exception Exception { get; set; }

    }
}