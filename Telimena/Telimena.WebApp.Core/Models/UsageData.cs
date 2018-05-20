namespace Telimena.WebApp.Core.Models
{
    using System;

    public abstract class UsageData
    {
        public int Id { get; set; }
        public DateTime LastUsageDateTime { get; set; } = DateTime.UtcNow;
        public virtual ClientAppUser ClientAppUser { get; set; }
        public int? UserInfoId { get; set; }
        public int Count { get; set; }

        public virtual void IncrementUsage()
        {
            this.Count++;
            this.LastUsageDateTime = DateTime.UtcNow;
        }
    }
}