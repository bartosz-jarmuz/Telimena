using System.Collections.Generic;
using System.Linq;
using DotNetLittleHelpers;
using TelimenaClient;

namespace Telimena.WebApp.Core.Models
{
    public class ViewTelemetrySummary : TelemetrySummary
    {
        public int ViewId { get; set; }
        public virtual View View { get; set; }
        public virtual RestrictedAccessList<ViewTelemetryDetail> TelemetryDetails { get; set; } = new RestrictedAccessList<ViewTelemetryDetail>();

        public override void AddTelemetryDetail(string ipAddress, AssemblyVersionInfo versionInfo, TelemetryItem telemetryItem)
        {
            ViewTelemetryDetail detail = new ViewTelemetryDetail(telemetryItem.Id)
            {
                Timestamp = telemetryItem.Timestamp, TelemetrySummary = this, AssemblyVersion = versionInfo, IpAddress = ipAddress
            };
            if (telemetryItem.TelemetryData != null && telemetryItem.TelemetryData.Any())
            {
                foreach (KeyValuePair<string, object> unit in telemetryItem.TelemetryData)
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