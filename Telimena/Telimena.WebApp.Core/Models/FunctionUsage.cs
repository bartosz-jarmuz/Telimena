namespace Telimena.WebApp.Core.Models
{
    public class FunctionUsage : UsageData
    {
        public int FunctionId { get; set; }
        public virtual Function Function { get; set; }
    }
}