using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using DotNetLittleHelpers;
using TelimenaClient;

namespace Telimena.WebApp.Core.Models
{
    public class Program : ITelemetryAware
    {
        protected Program()
        {
        }

        public Program(string name, Guid telemetryKey)
        {
            this.Name = name;
            this.TelemetryKey = telemetryKey;
            this.RegisteredDate = DateTime.UtcNow;
        }

        [Key]
        public int Id { get; set; }
        public Guid TelemetryKey { get; set; } = Guid.NewGuid();

        public ICollection<ProgramAssembly> ProgramAssemblies { get; set; } = new List<ProgramAssembly>();
        public virtual ProgramAssembly PrimaryAssembly { get; set; }
        public virtual ICollection<View> Views { get; set; } = new List<View>();
        public virtual ICollection<Event> Events { get; set; } = new List<Event>();
        public virtual RestrictedAccessList<ProgramTelemetrySummary> TelemetrySummaries { get; set; } = new RestrictedAccessList<ProgramTelemetrySummary>();
        public DateTime RegisteredDate { get; set; }

        [StringLength(255)]
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public virtual DeveloperAccount DeveloperAccount { get; set; }

        public virtual Updater Updater { get; set; }

        public IReadOnlyList<TelemetrySummary> GetTelemetrySummaries()
        {
            return this.TelemetrySummaries.AsReadOnly();
        }

        public IReadOnlyList<TelemetryDetail> GetTelemetryDetails(int clientAppUserId)
        {
            TelemetrySummary summary = this.GetTelemetrySummary(clientAppUserId);
            return summary?.GetTelemetryDetails();
        }

        public TelemetrySummary GetTelemetrySummary(int clientAppUserId)
        {
            return this.TelemetrySummaries.FirstOrDefault(x => x.ClientAppUser.Id == clientAppUserId);
        }

        public TelemetrySummary AddTelemetrySummary(int clientAppUserId)
        {
            ProgramTelemetrySummary summary = new ProgramTelemetrySummary {ClientAppUserId = clientAppUserId, Program = this};
            ((List<ProgramTelemetrySummary>) this.TelemetrySummaries).Add(summary);
            return summary;
        }

        public ProgramTelemetryDetail GetLatestTelemetryDetail()
        {
            ProgramTelemetrySummary summary = this.TelemetrySummaries.MaxFirstBy(x => x.LastReportedDateTime);
            return summary.GetTelemetryDetails().MaxFirstBy(x => x.Id) as ProgramTelemetryDetail;
        }

        public AssemblyVersionInfo GetLatestVersion()
        {
            return this.PrimaryAssembly?.GetLatestVersion();
        }

        internal bool UseAssemblyVersionAsProgramVersion = false;

        /// <summary>
        /// At some point we *might* implement a mechanism where program version is determined either by assembly version or file version depending on settings in the cloud 
        /// but for now, hardcode it to FileVersion
        /// </summary>
        /// <param name="versionData"></param>
        /// <returns></returns>
        public string DetermineProgramVersion(VersionData versionData)
        {
            if (this.UseAssemblyVersionAsProgramVersion)
            {
               return versionData.AssemblyVersion;
            }
            else
            {
                return versionData.FileVersion;
            }
        }
    }
}