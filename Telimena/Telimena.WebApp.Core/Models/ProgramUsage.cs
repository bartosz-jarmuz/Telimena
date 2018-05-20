namespace Telimena.WebApp.Core.Models
{
    public class ProgramUsage : UsageData
    {
        public int ProgramId { get; set; }
        public virtual Program Program { get; set; }

    }
}