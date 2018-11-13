using System;
using System.Collections.Generic;

namespace Telimena.WebApp.Core.Models
{
    public class EventTelemetrySummary : TelemetrySummary
    {
        public int EventId { get; set; }
        public virtual Event Event { get; set; }

        public IEnumerable<EventTelemetryDetail> Details { get; set; } = new List<EventTelemetryDetail>();

        public override IEnumerable<TelemetryDetail> TelemetryDetails => this.Details;

        public override void UpdateUsageDetails(DateTime lastUsageDateTime, string ipAddress, AssemblyVersionInfo versionInfo, string customData)
        {
            throw new NotImplementedException();
        }
    }
}