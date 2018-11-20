using System;
using System.Collections.Generic;
using DotNetLittleHelpers;

namespace Telimena.WebApp.Core.Models
{
    public class EventTelemetrySummary : TelemetrySummary
    {
        public int EventId { get; set; }
        public virtual Event Event { get; set; }

        public virtual RestrictedAccessList<EventTelemetryDetail> TelemetryDetails { get; set; } = new RestrictedAccessList<EventTelemetryDetail>();

        public override IReadOnlyList<TelemetryDetail> GetTelemetryDetails() => this.TelemetryDetails.AsReadOnly();

        public override void AddTelemetryDetail(DateTime lastUsageDateTime, string ipAddress, AssemblyVersionInfo versionInfo
            , Dictionary<string, string> telemetryUnits)
        {
            throw new NotImplementedException();
        }
    }
}