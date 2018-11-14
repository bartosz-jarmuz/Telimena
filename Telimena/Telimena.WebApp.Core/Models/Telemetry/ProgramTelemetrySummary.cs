using System;
using System.Collections.Generic;
using DotNetLittleHelpers;

namespace Telimena.WebApp.Core.Models
{
    public class ProgramTelemetrySummary : TelemetrySummary
    {
        public int ProgramId { get; set; }
        public virtual Program Program{ get; set; }

        public RestrictedAccessList<ProgramTelemetryDetail> TelemetryDetails { get; set; } = new RestrictedAccessList<ProgramTelemetryDetail>();

        public override IReadOnlyList<TelemetryDetail> GetTelemetryDetails() => this.TelemetryDetails.AsReadOnly();

        public override void UpdateUsageDetails(DateTime lastUsageDateTime, string ipAddress, AssemblyVersionInfo versionInfo
            , Dictionary<string, string> telemetryUnits)
        {
            ProgramTelemetryDetail detail = new ProgramTelemetryDetail
            {
                DateTime = lastUsageDateTime,
                TelemetrySummary = this,
                AssemblyVersion = versionInfo,
                IpAddress = ipAddress
            };
            ((List<ProgramTelemetryDetail>)this.TelemetryDetails).Add(detail);
        }
    }
}