using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Telimena.WebApp.Core.Models
{
    public abstract class TelemetrySummary
    {
        public int Id { get; set; }
        public DateTime LastReportedDateTime { get; set; } = DateTime.UtcNow;

        public virtual ClientAppUser ClientAppUser { get; set; }
        public int? ClientAppUserId { get; set; }

        private int summaryCount;

        public int SummaryCount
        {
            get
            {
                this.summaryCount = this.GetTelemetryDetails()?.Count() ?? 0;
                return this.summaryCount;
            }
            set => this.summaryCount = value;
        }
        public abstract IReadOnlyList<TelemetryDetail> GetTelemetryDetails();
        public abstract ITelemetryAware GetComponent();

        public abstract void AddTelemetryDetail(DateTime lastUsageDateTime, string ipAddress, AssemblyVersionInfo versionInfo
            , Dictionary<string, string> telemetryUnits);

        public virtual void UpdateTelemetry(AssemblyVersionInfo versionInfo, string ipAddress, Dictionary<string, string> telemetryUnits = null)
        {
            this.LastReportedDateTime = DateTime.UtcNow;
            this.AddTelemetryDetail(this.LastReportedDateTime, ipAddress, versionInfo, telemetryUnits);
        }
    }
}