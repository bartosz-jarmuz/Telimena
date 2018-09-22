namespace Telimena.WebApp.Core.Models
{
    public class ProgramUsageDetail : UsageDetail
    {
        public virtual ProgramUsageSummary UsageSummary { get; set; }
        public virtual ProgramCustomUsageData  CustomUsageData { get; set; }
    }
}