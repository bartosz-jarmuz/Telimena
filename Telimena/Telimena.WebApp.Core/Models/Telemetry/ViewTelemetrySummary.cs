using System;
using System.Collections.Generic;

namespace Telimena.WebApp.Core.Models
{
    public class ViewTelemetrySummary : TelemetrySummary
    {
        public int ViewId { get; set; }
        public virtual View View { get; set; }
        public ICollection<ViewTelemetryDetail> Details { get; set;  } = new List<ViewTelemetryDetail>();

        public override IEnumerable<TelemetryDetail> TelemetryDetails => this.Details;

        public override void UpdateUsageDetails(DateTime lastUsageDateTime, string ipAddress, AssemblyVersionInfo versionInfo, string customData)
        {
            ViewTelemetryDetail usage = new ViewTelemetryDetail
            {
                DateTime = lastUsageDateTime,
                UsageSummary = this,
                AssemblyVersion = versionInfo,
                IpAddress = ipAddress
            };
            this.Details.Add(usage);
        }
    }
}