using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using DotNetLittleHelpers;

namespace Telimena.WebApp.Core.Models
{

    public class ProgramTelemetryDetail : TelemetryDetail
    {
        public virtual ProgramTelemetrySummary TelemetrySummary { get; set; }
        public virtual RestrictedAccessList<ProgramTelemetryUnit> TelemetryUnits { get; set; } = new RestrictedAccessList<ProgramTelemetryUnit>();

        public override TelemetrySummary GetTelemetrySummary() => this.TelemetrySummary;

        public override IReadOnlyList<TelemetryUnit> GetTelemetryUnits() => this.TelemetryUnits.AsReadOnly();
    }
}