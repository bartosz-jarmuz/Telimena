namespace Telimena.WebApp.Core.Models
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public abstract class UsageDetail
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; } = DateTime.UtcNow;
        
        public int UsageSummaryId { get; set; }
    }
}