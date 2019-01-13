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
     
        public virtual RestrictedAccessList<EventTelemetryDetail> TelemetryDetails { get; set; } = new RestrictedAccessList<EventTelemetryDetail>();

        public override List<TelemetryDetail> GetTelemetryDetails() => this.TelemetryDetails.OfType<TelemetryDetail>().ToList();

        public override void AddTelemetryDetail(string ipAddress, AssemblyVersionInfo versionInfo, TelemetryItem telemetryItem)
        {
            EventTelemetryDetail detail = new EventTelemetryDetail()
            {
                Timestamp = telemetryItem.Timestamp,
                TelemetrySummary = this,
                AssemblyVersion = versionInfo,
                IpAddress = ipAddress
            };
            if (telemetryItem.TelemetryData != null && telemetryItem.TelemetryData.Any())
            {
                foreach (KeyValuePair<string, object> unit in telemetryItem.TelemetryData)
                {
                    EventTelemetryUnit telemetryUnit = new EventTelemetryUnit() { Key = unit.Key, ValueString = unit.Value?.ToString() };
                    ((List<EventTelemetryUnit>)detail.TelemetryUnits).Add(telemetryUnit);
                }
            }
            ((List<EventTelemetryDetail>)this.TelemetryDetails).Add(detail);
        }
    }
}