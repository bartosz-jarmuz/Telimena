using System;
using System.Collections.Generic;
using DotNetLittleHelpers;

namespace Telimena.WebApp.Core.Models.Telemetry
{
    public class Event : ProgramComponent
    {
        public virtual RestrictedAccessList<EventTelemetrySummary> TelemetrySummaries { get; set; } = new RestrictedAccessList<EventTelemetrySummary>();

        public override IReadOnlyList<TelemetrySummary> GetTelemetrySummaries() => this.TelemetrySummaries.AsReadOnly();

        public override TelemetrySummary AddTelemetrySummary(int clientAppUserId)
        {
            EventTelemetrySummary summary = new EventTelemetrySummary()
            {
                ClientAppUserId = clientAppUserId,
                Event = this
            };
            ((List<EventTelemetrySummary>)this.TelemetrySummaries).Add(summary);
            return summary;
        }
    }
}