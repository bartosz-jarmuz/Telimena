using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        public virtual ICollection<View> Views { get; set; } = new List<View>();
        public virtual ICollection<Event> TrackedEvents { get; set; } = new List<Event>();
        public virtual ICollection<ProgramTelemetrySummary> UsageSummaries { get; set; } = new List<ProgramTelemetrySummary>();
        public DateTime RegisteredDate { get; set; }

        [StringLength(450)]
        [Index(IsUnique = true)]
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public virtual DeveloperAccount DeveloperAccount { get; set; }

        public virtual Updater Updater { get; set; }

        public ProgramTelemetryDetail GetLatestUsageDetail()
        {
            var summary = this.UsageSummaries.MaxFirstBy(x => x.LastUsageDateTime);
            return summary.TelemetryDetails.MaxFirstBy(x => x.Id) as ProgramTelemetryDetail;
        }

        public AssemblyVersionInfo GetLatestVersion()
        {
            return this.PrimaryAssembly?.GetLatestVersion();
        }

        public ICollection<ProgramTelemetryDetail> GetProgramUsageDetails(int clientAppUserId)
        {
            ProgramTelemetrySummary usage = this.GetProgramUsageSummary(clientAppUserId);
            return usage.TelemetryDetails.Cast<ProgramTelemetryDetail>().ToList();
        }

        public ProgramTelemetrySummary GetProgramUsageSummary(int clientAppUserId)
        {
            return this.UsageSummaries.FirstOrDefault(x => x.ClientAppUser.Id == clientAppUserId);
        }
    }
}