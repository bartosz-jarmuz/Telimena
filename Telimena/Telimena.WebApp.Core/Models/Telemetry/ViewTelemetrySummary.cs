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
        public virtual RestrictedAccessList<ViewTelemetryDetail> TelemetryDetails { get; set;  } = new RestrictedAccessList<ViewTelemetryDetail>();

        public override List<TelemetryDetail> GetTelemetryDetails() => this.TelemetryDetails.OfType<TelemetryDetail>().ToList();
        public override ITelemetryAware GetComponent() => this.View;

        public override void AddTelemetryDetail(string ipAddress, AssemblyVersionInfo versionInfo, TelemetryItem telemetryItem)
        {
            ViewTelemetryDetail detail = new ViewTelemetryDetail
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
                    var telemetryUnit = new ViewTelemetryUnit() { Key = unit.Key, ValueString = unit.Value?.ToString() };
                    ((List<ViewTelemetryUnit>)detail.TelemetryUnits).Add(telemetryUnit);
                }
            }
            ((List<ViewTelemetryDetail>)this.TelemetryDetails).Add(detail);
        }

    }
}