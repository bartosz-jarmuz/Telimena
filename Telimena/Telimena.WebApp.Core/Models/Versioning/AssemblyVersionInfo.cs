using System;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using TelimenaClient;

namespace Telimena.WebApp.Core.Models
{
    public class AssemblyVersionInfo
    {
        [Obsolete("Constructor required by EF")]
        protected AssemblyVersionInfo() { }

        public AssemblyVersionInfo(VersionData versionData)
        {
            this.AssemblyVersion = versionData.AssemblyVersion;
            this.FileVersion = versionData.FileVersion;
        }

        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the version from the [assembly: AssemblyVersion("1.0")] attribute
        /// <para/><seealso href="https://stackoverflow.com/questions/64602/what-are-differences-between-assemblyversion-assemblyfileversion-and-assemblyin"/> 
        /// </summary>
        /// <value>The version.</value>
        public string AssemblyVersion { get; set; }

        /// <summary>
        /// The file version which is set by [assembly: AssemblyFileVersion("1.0.0.0")] attribute
        /// <para/><seealso href="https://stackoverflow.com/questions/64602/what-are-differences-between-assemblyversion-assemblyfileversion-and-assemblyin"/> 
        /// </summary>
        public string FileVersion { get; set; }

        public DateTime? ReleaseDate { get; set; }

        [JsonIgnore]
        public virtual ProgramAssembly ProgramAssembly { get; set; }

        public int ProgramAssemblyId { get; set; }

        [JsonIgnore]
        public virtual ProgramAssembly LatestVersionOf { get; set; }

        public virtual TelimenaToolkitData ToolkitData { get; set; }
    }
}