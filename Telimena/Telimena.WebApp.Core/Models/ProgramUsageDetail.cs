namespace Telimena.WebApp.Core.Models
{
    public class ProgramUsageDetail : UsageDetail
    {
        public virtual ProgramUsageSummary UsageSummary { get; set; }
    }
}