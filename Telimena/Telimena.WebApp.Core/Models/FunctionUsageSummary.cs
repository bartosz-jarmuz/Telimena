namespace Telimena.WebApp.Core.Models
{
    public class FunctionUsageSummary : UsageSummary
    {
        public int FunctionId { get; set; }
        public virtual Function Function { get; set; }
    }


}