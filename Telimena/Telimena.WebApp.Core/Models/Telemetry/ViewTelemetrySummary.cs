using System.Collections.Generic;
using System.Linq;
using DotNetLittleHelpers;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.DTO.MappableToClient;

namespace Telimena.WebApp.Core.Models
{
    public class ViewTelemetrySummary : TelemetrySummary
    {
        public int ViewId { get; set; }
        public virtual View View { get; set; }
        public virtual RestrictedAccessList<ViewTelemetryDetail> TelemetryDetails { get; set; } = new RestrictedAccessList<ViewTelemetryDetail>();

        public override void AddTelemetryDetail(string ipAddress, VersionData versionInfo, TelemetryItem telemetryItem)
        {
            ViewTelemetryDetail detail = new ViewTelemetryDetail(telemetryItem.Id)
            {
                Timestamp = telemetryItem.Timestamp, TelemetrySummary = this, AssemblyVersion = versionInfo.AssemblyVersion, FileVersion = versionInfo.FileVersion, IpAddress = ipAddress,
                Sequence = telemetryItem.Sequence,
                UserId = telemetryItem.UserId,
                EntryKey = telemetryItem.EntryKey
            };
            if (telemetryItem.TelemetryData != null && telemetryItem.TelemetryData.Any())
            {
                foreach (KeyValuePair<string, string> unit in telemetryItem.TelemetryData)
                {
                    ViewTelemetryUnit telemetryUnit = new ViewTelemetryUnit {Key = unit.Key, ValueString = unit.Value?.ToString()};
                    ((List<ViewTelemetryUnit>) detail.TelemetryUnits).Add(telemetryUnit);
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