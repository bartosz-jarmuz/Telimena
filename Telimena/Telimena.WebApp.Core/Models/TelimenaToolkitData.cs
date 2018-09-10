using System.Collections.Generic;
using DotNetLittleHelpers;

namespace Telimena.WebApp.Core.Models
{
    using System;

    public class TelimenaToolkitData 
    {
        public TelimenaToolkitData() { }
        public int Id { get; set; }
        public string Version { get; set; }
        public DateTime ReleaseDate { get; set; }

        public UpdaterInfo UpdaterInfo { get; set; }

        public virtual RestrictedAccessCollection<AssemblyVersion> RelatedAssemblies { get; set; } = new List<AssemblyVersion>();

    }
}
