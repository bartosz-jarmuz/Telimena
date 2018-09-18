using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DotNetLittleHelpers;

namespace Telimena.WebApp.Core.Models
{
    public class Program
    {
        protected Program()
        {
        }

        public Program(string name)
        {
            this.Name = name;
        }

        public int Id { get; set; }
        public ICollection<ProgramAssembly> ProgramAssemblies { get; set; } = new List<ProgramAssembly>();
        public virtual ProgramAssembly PrimaryAssembly { get; set; }
        public virtual ICollection<Function> Functions { get; set; } = new List<Function>();
        public virtual ICollection<ProgramUsageSummary> UsageSummaries { get; set; } = new List<ProgramUsageSummary>();
        public DateTime RegisteredDate { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public virtual DeveloperAccount DeveloperAccount { get; set; }

        public ProgramUsageDetail GetLatestUsageDetail()
        {
            ProgramUsageSummary summary = this.UsageSummaries.MaxFirstBy(x => x.LastUsageDateTime);
            return summary.UsageDetails.MaxFirstBy(x => x.Id);
        }

        public AssemblyVersion GetLatestVersion()
        {
            return this.PrimaryAssembly?.LatestVersion;
        }

        public ICollection<ProgramUsageDetail> GetProgramUsageDetails(int clientAppUserId)
        {
            ProgramUsageSummary usage = this.GetProgramUsageSummary(clientAppUserId);
            return usage.UsageDetails;
        }

        public ProgramUsageSummary GetProgramUsageSummary(int clientAppUserId)
        {
            return this.UsageSummaries.FirstOrDefault(x => x.ClientAppUser.Id == clientAppUserId);
        }
    }
}