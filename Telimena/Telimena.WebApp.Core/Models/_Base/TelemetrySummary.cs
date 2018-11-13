using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Telimena.WebApp.Core.Models
{
    public abstract class TelemetrySummary
    {
        public int Id { get; set; }
        public DateTime LastUsageDateTime { get; set; } = DateTime.UtcNow;

        public virtual ClientAppUser ClientAppUser { get; set; }
        public int? ClientAppUserId { get; set; }

        [NotMapped]
        public abstract IEnumerable<TelemetryDetail> TelemetryDetails {  get; }

        private int summaryCount;

        public int SummaryCount
        {
            get
            {
                this.summaryCount = this.TelemetryDetails?.Count() ?? 0;
                return this.summaryCount;
            }
            set => this.summaryCount = value;
        }

        public abstract void UpdateUsageDetails(DateTime lastUsageDateTime, string ipAddress, AssemblyVersionInfo versionInfo, string customData);

        public virtual void IncrementUsage(AssemblyVersionInfo versionInfo, string ipAddress, string customData = null)
        {
            this.LastUsageDateTime = DateTime.UtcNow;
            this.UpdateUsageDetails(this.LastUsageDateTime, ipAddress, versionInfo, customData);
        }
    }
}