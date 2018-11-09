using System;

namespace Telimena.WebApp.Core.Models
{
    public abstract class UsageSummary
    {
        public abstract int SummaryCount { get; set; }
        public int Id { get; set; }
        public DateTime LastUsageDateTime { get; set; } = DateTime.UtcNow;
        public virtual ClientAppUser ClientAppUser { get; set; }
        public int? ClientAppUserId { get; set; }

        public abstract void UpdateUsageDetails(DateTime lastUsageDateTime, string ipAddress, AssemblyVersionInfo versionInfo, string customData);

        public virtual void IncrementUsage(AssemblyVersionInfo versionInfo, string ipAddress, string customData = null)
        {
            this.LastUsageDateTime = DateTime.UtcNow;
            this.UpdateUsageDetails(this.LastUsageDateTime, ipAddress, versionInfo, customData);
        }
    }
}