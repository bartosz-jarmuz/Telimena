using System.Collections.Generic;

namespace Telimena.WebApp.Core.Models
{
    public class ViewTelemetryDetail : TelemetryDetail
    {
        public virtual ViewTelemetrySummary UsageSummary { get; set; }
        public virtual IEnumerable<ViewTelemetryUnit> Units { get; set; } = new List<ViewTelemetryUnit>();
        public override IEnumerable<TelemetryUnit> TelemetryUnits => this.Units;
    }
}