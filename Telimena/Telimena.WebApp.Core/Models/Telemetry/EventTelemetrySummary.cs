using System;
using System.Collections.Generic;
using System.Linq;
using DotNetLittleHelpers;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.DTO.MappableToClient;

namespace Telimena.WebApp.Core.Models.Telemetry
{
    public class EventTelemetrySummary : TelemetrySummary
    {
        public Guid EventId { get; set; }
        public virtual Event Event { get; set; }

        public virtual RestrictedAccessList<EventTelemetryDetail> TelemetryDetails { get; set; } = new RestrictedAccessList<EventTelemetryDetail>();

        public override void 
            AddTelemetryDetail(string ipAddress, VersionData versionInfo, TelemetryItem telemetryItem)
        {
            EventTelemetryDetail detail = new EventTelemetryDetail(telemetryItem.Id)
            {
                Timestamp = telemetryItem.Timestamp,
                TelemetrySummary = this,
                AssemblyVersion = versionInfo.AssemblyVersion,
                FileVersion = versionInfo.FileVersion,
                IpAddress = ipAddress,
                Sequence = telemetryItem.Sequence,
                UserIdentifier = telemetryItem.UserIdentifier,
                EntryKey = telemetryItem.EntryKey
            };
            if (telemetryItem.Properties != null && telemetryItem.Properties.Any())
            {
                foreach (KeyValuePair<string, string> unit in telemetryItem.Properties)
                {
                    EventTelemetryUnit telemetryUnit = new EventTelemetryUnit {Key = unit.Key, ValueString = unit.Value?.ToString(), UnitType = TelemetryUnit.UnitTypes.Property};
                    ((List<EventTelemetryUnit>) detail.TelemetryUnits).Add(telemetryUnit);
                }
            }
            if (telemetryItem.Measurements != null && telemetryItem.Measurements.Any())
            {
                foreach (KeyValuePair<string, double> unit in telemetryItem.Measurements)
                {
                    EventTelemetryUnit telemetryUnit = new EventTelemetryUnit { Key = unit.Key, ValueDouble = unit.Value, UnitType = TelemetryUnit.UnitTypes.Metric };
                    ((List<EventTelemetryUnit>)detail.TelemetryUnits).Add(telemetryUnit);
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