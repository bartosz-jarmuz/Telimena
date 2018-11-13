using System;
using System.Collections.Generic;

namespace Telimena.WebApp.Core.Models
{
    public class ProgramTelemetrySummary : TelemetrySummary
    {
        public int ProgramId { get; set; }
        public virtual Program Program{ get; set; }

        public ICollection<ProgramTelemetryDetail> Details { get; set; } = new List<ProgramTelemetryDetail>();
        public override IEnumerable<TelemetryDetail> TelemetryDetails => this.Details;

        public override void UpdateUsageDetails(DateTime lastUsageDateTime, string ipAddress, AssemblyVersionInfo versionInfo, string customData)
        {
            ProgramTelemetryDetail usage = new ProgramTelemetryDetail
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