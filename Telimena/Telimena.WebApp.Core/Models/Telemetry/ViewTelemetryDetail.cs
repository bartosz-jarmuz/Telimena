using System.Collections.Generic;
using DotNetLittleHelpers;

namespace Telimena.WebApp.Core.Models
{
    public class ViewTelemetryDetail : TelemetryDetail
    {
        public virtual ViewTelemetrySummary TelemetrySummary { get; set; }
        public virtual RestrictedAccessList<ViewTelemetryUnit> TelemetryUnits { get; set; } = new RestrictedAccessList<ViewTelemetryUnit>();

        public override IReadOnlyList<TelemetryUnit> GetTelemetryUnits() => this.TelemetryUnits.AsReadOnly();

        public override TelemetrySummary GetTelemetrySummary() => this.TelemetrySummary;

    }
}