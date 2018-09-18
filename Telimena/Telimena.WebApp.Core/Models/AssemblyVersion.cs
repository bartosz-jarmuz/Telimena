using System;
using Newtonsoft.Json;

namespace Telimena.WebApp.Core.Models
{
    public class AssemblyVersion
    {
        public int Id { get; set; }
        public string Version { get; set; }
        public DateTime? ReleaseDate { get; set; }

        [JsonIgnore]
        public virtual ProgramAssembly ProgramAssembly { get; set; }

        public int ProgramAssemblyId { get; set; }

        [JsonIgnore]
        public virtual ProgramAssembly LatestVersionOf { get; set; }

        public virtual TelimenaToolkitData ToolkitData { get; set; }
    }
}