using System.Collections.Generic;
using System.Linq;
using DotNetLittleHelpers;
using TelimenaClient;

namespace Telimena.WebApp.Core.Models
{
    public class ProgramTelemetrySummary : TelemetrySummary
    {
        public int ProgramId { get; set; }
        public virtual Program Program{ get; set; }
        public override ITelemetryAware GetComponent() => this.Program;
        public override TelemetryDetail CreateNewDetail()
        {
            return new ProgramTelemetryDetail();
        }

        public virtual RestrictedAccessList<ProgramTelemetryDetail> TelemetryDetails { get; set; } = new RestrictedAccessList<ProgramTelemetryDetail>();

        public override List<TelemetryDetail> GetTelemetryDetails() => this.TelemetryDetails.OfType<TelemetryDetail>().ToList();

        public void AddTelemetryDetail(string ipAddress, AssemblyVersionInfo versionInfo, TelemetryItem telemetryUnits)
        {
            ProgramTelemetryDetail detail = new ProgramTelemetryDetail
            {
                Timestamp = telemetryUnits.Timestamp,
                TelemetrySummary = this,
                AssemblyVersion = versionInfo,
                IpAddress = ipAddress
            };
            ((List<ProgramTelemetryDetail>)this.TelemetryDetails).Add(detail);
        }
    }
}