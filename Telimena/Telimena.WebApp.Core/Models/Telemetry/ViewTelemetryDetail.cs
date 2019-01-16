using System;
using System.Collections.Generic;
using DotNetLittleHelpers;

namespace Telimena.WebApp.Core.Models
{
    public class ViewTelemetryDetail : TelemetryDetail
    {
        public ViewTelemetryDetail(Guid id) : base(id)
        {
        }

        public virtual ViewTelemetrySummary TelemetrySummary { get; set; }
        public virtual RestrictedAccessList<ViewTelemetryUnit> TelemetryUnits { get; set; } = new RestrictedAccessList<ViewTelemetryUnit>();

        public override TelemetrySummary GetTelemetrySummary()
        {
            return this.TelemetrySummary;
        }

        public override IReadOnlyList<TelemetryUnit> GetTelemetryUnits()
        {
            return this.TelemetryUnits.AsReadOnly();
        }

        public override void SetTelemetrySummary(TelemetrySummary summary)
        {
            this.TelemetrySummary = (ViewTelemetrySummary) summary;
        }
    }
}