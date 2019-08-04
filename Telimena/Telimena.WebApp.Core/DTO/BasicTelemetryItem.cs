using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Telimena.WebApp.Core.DTO
{
    /// <summary>
    /// Class BasicTelemetryRequest.
    /// </summary>
    public class BasicTelemetryItem
    {
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>The user identifier.</value>
        public string UserIdentifier { get; set; } = "Unspecified";

        /// <summary>
        /// Gets or sets the name of the event (or view).
        /// </summary>
        /// <value>The name of the event.</value>
        public string EventName { get; set; } = "DefaultEvent";
        /// <summary>
        /// Gets or sets the type of the telemetry item.
        /// </summary>
        /// <value>The type of the telemetry item.</value>
        public TelemetryItemTypes TelemetryItemType { get; set; } = TelemetryItemTypes.Event;
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, double> Metrics { get; set; } = new Dictionary<string, double>();
        public string ProgramVersion { get; set; } = "0.0.0.0";
        public string LogMessage { get; set; } = "";
        public string ErrorMessage { get; set; } = "";
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    }
}