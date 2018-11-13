using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telimena.WebApp.Core.Models
{
    [Table("ProgramTelemetryDetails")]

    public class ProgramTelemetryDetail : TelemetryDetail
    {
        public virtual ProgramTelemetrySummary UsageSummary { get; set; }
        public virtual IEnumerable<ProgramTelemetryUnit> Units { get; set; } = new List<ProgramTelemetryUnit>();

        public override IEnumerable<TelemetryUnit> TelemetryUnits => this.Units;
    }
}