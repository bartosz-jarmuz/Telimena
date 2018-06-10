namespace Telimena.WebApp.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public class Program
    {
        public int Id { get; set; }
        [Required]
        public virtual PrimaryAssembly PrimaryAssembly { get; set; }
        public virtual ICollection<ReferencedAssembly> Assemblies { get; set; } = new List<ReferencedAssembly>();
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

    }
}