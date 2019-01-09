using System;
using System.Collections.Generic;
using System.Linq;
using DotNetLittleHelpers;
using TelimenaClient;

namespace Telimena.WebApp.Core.Models
{
    public class EventTelemetrySummary : TelemetrySummary
    {
        public int EventId { get; set; }
        public virtual Event Event { get; set; }
        public override ITelemetryAware GetComponent() => this.Event;
        public override TelemetryDetail CreateNewDetail()
        {
            return new EventTelemetryDetail();
        }

        public virtual RestrictedAccessList<EventTelemetryDetail> TelemetryDetails { get; set; } = new RestrictedAccessList<EventTelemetryDetail>();

        public override List<TelemetryDetail> GetTelemetryDetails() => this.TelemetryDetails.OfType<TelemetryDetail>().ToList();

        
    }
}