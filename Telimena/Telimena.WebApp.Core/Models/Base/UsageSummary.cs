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
        public int Count { get; set; }

       // public virtual ICollection<UsageDetail> UsageDetails { get; set; } = new List<UsageDetail>();

        public virtual void IncrementUsage()
        {
            this.Count++;
            this.LastUsageDateTime = DateTime.UtcNow;
        }
    }
}