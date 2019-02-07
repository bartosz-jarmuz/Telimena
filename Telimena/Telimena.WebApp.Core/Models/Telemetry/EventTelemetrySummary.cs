using System.Collections.Generic;
using System.Linq;
using DotNetLittleHelpers;
using Telimena.WebApp.Core.DTO;

namespace Telimena.WebApp.Core.Models
{
    public class EventTelemetrySummary : TelemetrySummary
    {
        public int EventId { get; set; }
        public virtual Event Event { get; set; }

        public virtual RestrictedAccessList<EventTelemetryDetail> TelemetryDetails { get; set; } = new RestrictedAccessList<EventTelemetryDetail>();

        public override void AddTelemetryDetail(string ipAddress, AssemblyVersionInfo versionInfo, TelemetryItem telemetryItem)
        {
            EventTelemetryDetail detail = new EventTelemetryDetail(telemetryItem.Id)
            {
                Timestamp = telemetryItem.Timestamp, TelemetrySummary = this, AssemblyVersion = versionInfo, IpAddress = ipAddress, Sequence = telemetryItem.Sequence
            };
            if (telemetryItem.TelemetryData != null && telemetryItem.TelemetryData.Any())
            {
                foreach (KeyValuePair<string, string> unit in telemetryItem.TelemetryData)
                {
                    EventTelemetryUnit telemetryUnit = new EventTelemetryUnit {Key = unit.Key, ValueString = unit.Value?.ToString()};
                    ((List<EventTelemetryUnit>) detail.TelemetryUnits).Add(telemetryUnit);
                }
            }

            ((List<EventTelemetryDetail>) this.TelemetryDetails).Add(detail);
        }

        public override ITelemetryAware GetComponent()
        {
            return this.Event;
        }

        public override List<TelemetryDetail> GetTelemetryDetails()
        {
            return this.TelemetryDetails.OfType<TelemetryDetail>().ToList();
        }
    }
}