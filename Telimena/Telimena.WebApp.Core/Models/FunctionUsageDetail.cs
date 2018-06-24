namespace Telimena.WebApp.Core.Models
{
    public class FunctionUsageDetail : UsageDetail
    {
        public virtual FunctionUsageSummary UsageSummary { get; set; }
    }
}