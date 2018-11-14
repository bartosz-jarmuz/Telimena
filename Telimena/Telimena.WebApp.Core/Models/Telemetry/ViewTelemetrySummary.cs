using System;
using System.Collections.Generic;
using System.Linq;
using DotNetLittleHelpers;

namespace Telimena.WebApp.Core.Models
{
    public class ViewTelemetrySummary : TelemetrySummary
    {
        public int ViewId { get; set; }
        public virtual View View { get; set; }
        public RestrictedAccessList<ViewTelemetryDetail> TelemetryDetails { get; set;  } = new RestrictedAccessList<ViewTelemetryDetail>();

        public override IReadOnlyList<TelemetryDetail> GetTelemetryDetails() => this.TelemetryDetails.AsReadOnly();

        public override void UpdateUsageDetails(DateTime lastUsageDateTime, string ipAddress, AssemblyVersionInfo versionInfo, Dictionary<string, string> telemetryUnits)
        {
            ViewTelemetryDetail detail = new ViewTelemetryDetail
            {
                DateTime = lastUsageDateTime,
                TelemetrySummary = this,
                AssemblyVersion = versionInfo,
                IpAddress = ipAddress
            };
            if (telemetryUnits != null && telemetryUnits.Any())
            {
                foreach (KeyValuePair<string, string> unit in telemetryUnits)
                {
                    var telemetryUnit = new ViewTelemetryUnit() {Key = unit.Key, Value = unit.Value};
                    ((List<ViewTelemetryUnit>)detail.TelemetryUnits).Add(telemetryUnit);
                }
            }

            ((List<ViewTelemetryDetail>)this.TelemetryDetails).Add(detail);
        }
    }
}