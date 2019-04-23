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
        public string UserIdentifier { get; set; } = "Unspecified";
        public string EventName { get; set; } = "DefaultEvent";
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, double> Metrics { get; set; } = new Dictionary<string, double>();
        public string ProgramVersion { get; set; } = "0.0.0.0";
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    }
}