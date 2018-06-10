namespace Telimena.WebApp.Core.Models
{
    using System;
    using System.Collections.Generic;

    public abstract class UsageSummary
    {
        public int Id { get; set; }
        public DateTime LastUsageDateTime { get; set; } = DateTime.UtcNow;
        public virtual ClientAppUser ClientAppUser { get; set; }
        public int? ClientAppUserId { get; set; }
        public int SummaryCount { get; set; }

        public abstract void UpdateUsageDetails(DateTime lastUsageDateTime);

        public virtual void IncrementUsage()
        {
            this.SummaryCount++;
            this.LastUsageDateTime = DateTime.UtcNow;
            this.UpdateUsageDetails(this.LastUsageDateTime);
        }
    }
}