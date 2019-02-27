using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using Telimena.WebApp.Core.DTO.MappableToClient;

namespace Telimena.WebApp.Core.Models.Portal
{
    public class Program 
    {
        protected Program()
        {
        }

        public Program(string name, Guid telemetryKey)
        {
            this.Name = name;
            this.TelemetryKey = telemetryKey;
            this.RegisteredDate = DateTimeOffset.UtcNow;
        }

        [Key]
        public int Id { get; set; }

        [Index(IsUnique = true, IsClustered = false)]
        public Guid PublicId { get; set; } = Guid.NewGuid();

        [Index(IsUnique = true, IsClustered = false)]
        public Guid TelemetryKey { get; set; } = Guid.NewGuid();

        public virtual ProgramAssembly PrimaryAssembly { get; set; }
      
        public DateTimeOffset RegisteredDate { get; set; }

        [StringLength(255)]
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public virtual DeveloperTeam DeveloperTeam { get; set; }

        public virtual Updater Updater { get; set; }

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

        /// <summary>
        /// At some point we *might* implement a mechanism where program version is determined either by assembly version or file version depending on settings in the cloud 
        /// but for now, hardcode it to FileVersion
        /// </summary>
        /// <param name="versionData"></param>
        /// <returns></returns>
        public string DetermineProgramVersion(AssemblyVersionInfo versionData)
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