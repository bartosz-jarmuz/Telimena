using System;
using System.Collections.Generic;
using DotNetLittleHelpers;

namespace Telimena.WebApp.Core.Models
{
    public class EventTelemetryDetail : TelemetryDetail
    {
        [Obsolete("EF")]
        public EventTelemetryDetail()
        {
        }

        public EventTelemetryDetail(Guid id) : base(id)
        {
        }

        public virtual EventTelemetrySummary TelemetrySummary { get; set; }
        public virtual RestrictedAccessList<EventTelemetryUnit> TelemetryUnits { get; set; } = new RestrictedAccessList<EventTelemetryUnit>();

        public override TelemetrySummary GetTelemetrySummary()
        {
            return this.TelemetrySummary;
        }

        public override IReadOnlyList<TelemetryUnit> GetTelemetryUnits()
        {
            return this.TelemetryUnits.AsReadOnly();
        }

    }
}