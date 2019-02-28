using System;
using System.Collections.Generic;
using System.Linq;
using DotNetLittleHelpers;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.DTO.MappableToClient;

namespace Telimena.WebApp.Core.Models.Telemetry
{
    public class ViewTelemetrySummary : TelemetrySummary
    {
        public Guid ViewId { get; set; }
        public virtual View View { get; set; }
        public virtual RestrictedAccessList<ViewTelemetryDetail> TelemetryDetails { get; set; } = new RestrictedAccessList<ViewTelemetryDetail>();

        public override void AddTelemetryDetail(string ipAddress, VersionData versionInfo, TelemetryItem telemetryItem)
        {
            ViewTelemetryDetail detail = new ViewTelemetryDetail(telemetryItem.Id)
            {
                Timestamp = telemetryItem.Timestamp, TelemetrySummary = this, AssemblyVersion = versionInfo.AssemblyVersion, FileVersion = versionInfo.FileVersion, IpAddress = ipAddress,
                Sequence = telemetryItem.Sequence,
                UserIdentifier = telemetryItem.UserIdentifier,
                EntryKey = telemetryItem.EntryKey
            };
            if (telemetryItem.Properties != null && telemetryItem.Properties.Any())
            {
                foreach (KeyValuePair<string, string> unit in telemetryItem.Properties)
                {
                    ViewTelemetryUnit telemetryUnit = new ViewTelemetryUnit {Key = unit.Key, ValueString = unit.Value?.ToString(), UnitType = TelemetryUnit.UnitTypes.Property};
                    ((List<ViewTelemetryUnit>) detail.TelemetryUnits).Add(telemetryUnit);
                }
            }
            if (telemetryItem.Measurements != null && telemetryItem.Measurements.Any())
            {
                foreach (KeyValuePair<string, double> unit in telemetryItem.Measurements)
                {
                    ViewTelemetryUnit telemetryUnit = new ViewTelemetryUnit() { Key = unit.Key, ValueDouble = unit.Value, UnitType = TelemetryUnit.UnitTypes.Metric };
                    ((List<ViewTelemetryUnit>)detail.TelemetryUnits).Add(telemetryUnit);
                }
            }
            ((List<ViewTelemetryDetail>) this.TelemetryDetails).Add(detail);
        }

        public override ITelemetryAware GetComponent()
        {
            return this.View;
        }

        public override List<TelemetryDetail> GetTelemetryDetails()
        {
            return this.TelemetryDetails.OfType<TelemetryDetail>().ToList();
        }
    }
}