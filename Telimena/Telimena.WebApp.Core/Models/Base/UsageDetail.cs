using System;

namespace Telimena.WebApp.Core.Models
{
    public abstract class UsageDetail
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; } = DateTime.UtcNow;
        public int UsageSummaryId { get; set; }

        public int? AssemblyVersionId { get; set; }
        public string IpAddress { get; set; }

        public virtual AssemblyVersion AssemblyVersion { get; set; }
    }
}