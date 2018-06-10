namespace Telimena.WebApp.Core.Models
{
    using System.Collections.Generic;

    public class ProgramUsageSummary : UsageSummary
    {
        public int ProgramId { get; set; }
        public virtual Program Program { get; set; }

    }


}