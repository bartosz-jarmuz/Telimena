namespace Telimena.WebApp.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    public abstract class UsageSummary
    {
        public int Id { get; set; }
        public DateTime LastUsageDateTime { get; set; } = DateTime.UtcNow;
        public virtual ClientAppUser ClientAppUser { get; set; }
        public int? ClientAppUserId { get; set; }
        public int SCount { get; set; }
        public abstract int SummaryCount { get; set; }

        public abstract void UpdateUsageDetails(DateTime lastUsageDateTime, AssemblyVersion version);

        public virtual void IncrementUsage(AssemblyVersion version)
        {
            this.SCount++;
            this.LastUsageDateTime = DateTime.UtcNow;
            this.UpdateUsageDetails(this.LastUsageDateTime, version);
        }

       
    }
}