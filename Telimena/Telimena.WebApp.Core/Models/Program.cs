namespace Telimena.WebApp.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using DotNetLittleHelpers;

    public class Program
    {
        public int ProgramId { get; set; }
        public ICollection<ProgramAssembly> ProgramAssemblies { get; set; } = new List<ProgramAssembly>();
        public virtual ProgramAssembly PrimaryAssembly { get; set; }

        public virtual ICollection<Function> Functions { get; set; } = new List<Function>();
        public virtual ICollection<ProgramUsageSummary> UsageSummaries { get; set; } = new List<ProgramUsageSummary>();
        public DateTime RegisteredDate { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }

        public Developer Developer { get; set; }

        public ProgramUsageSummary GetProgramUsageSummary(int clientAppUserId)
        {
            return this.UsageSummaries.FirstOrDefault(x => x.ClientAppUser.Id == clientAppUserId);
        }
        public ICollection<ProgramUsageDetail> GetProgramUsageDetails(int clientAppUserId)
        {
            var usage = this.GetProgramUsageSummary(clientAppUserId);
            return usage.UsageDetails;
        }

        public ProgramUsageDetail GetLatestUsageDetail()
        {
            ProgramUsageSummary summary = this.UsageSummaries.MaxFirstBy(x => x.LastUsageDateTime);
            return summary.UsageDetails.MaxFirstBy(x => x.Id);
        }

    }
}