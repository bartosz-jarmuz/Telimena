using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Telimena.WebApp.Core.DTO;

namespace Telimena.WebApp.Core.Models
{
    public abstract class TelemetrySummary
    {
        public int Id { get; set; }
        public DateTimeOffset LastTelemetryUpdateTimestamp { get; set; } = DateTime.UtcNow;

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

        public abstract List<TelemetryDetail> GetTelemetryDetails();
        public abstract ITelemetryAware GetComponent();

        public abstract void AddTelemetryDetail(string ipAddress, AssemblyVersionInfo versionInfo, TelemetryItem telemetryItem);

        public virtual void UpdateTelemetry(AssemblyVersionInfo versionInfo, string ipAddress, TelemetryItem telemetryItem)
        {
            this.LastTelemetryUpdateTimestamp = DateTimeOffset.UtcNow;
            this.AddTelemetryDetail(ipAddress, versionInfo, telemetryItem);
        }
    }
}