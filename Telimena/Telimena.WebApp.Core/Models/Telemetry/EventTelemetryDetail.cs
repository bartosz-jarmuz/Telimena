using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using DotNetLittleHelpers;

namespace Telimena.WebApp.Core.Models
{
    public class EventTelemetryDetail : TelemetryDetail
    {
        public virtual EventTelemetrySummary TelemetrySummary { get; set; }
        public virtual RestrictedAccessList<EventTelemetryUnit> TelemetryUnits { get; set; } = new RestrictedAccessList<EventTelemetryUnit>();

        public override TelemetrySummary GetTelemetrySummary() => this.TelemetrySummary;

        public override IReadOnlyList<TelemetryUnit> GetTelemetryUnits() => this.TelemetryUnits.AsReadOnly();
    }
}