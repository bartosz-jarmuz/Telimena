using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telimena.WebApp.Core.Models
{
    [Table("EventTelemetryDetails")]

    public class EventTelemetryDetail : TelemetryDetail
    {
        public virtual EventTelemetrySummary TelemetrySummary { get; set; }
        public virtual IEnumerable<EventTelemetryUnit> Units { get; set; } = new List<EventTelemetryUnit>();

        public override IEnumerable<TelemetryUnit> TelemetryUnits => this.Units;
    }
}