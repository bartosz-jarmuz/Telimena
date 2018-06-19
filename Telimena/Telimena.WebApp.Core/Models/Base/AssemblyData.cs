namespace Telimena.WebApp.Core.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using DotNetLittleHelpers;

    public abstract class AssemblyData
    {
        public string Name { get; set; }
        public string Product { get; set; }
        public string Trademark { get; set; }
        public string Description { get; set; }
        public string Copyright { get; set; }
        public string Title { get; set; }
        public string Company { get; set; }
        public string FullName { get; set; }
        public virtual ICollection<AssemblyVersion> Versions { get; set; } = new List<AssemblyVersion>();
        public AssemblyVersion GetLatestVersion()
        {
            return this.Versions?.OrderBy(x => x.Version, new VersionStringComparer()).FirstOrDefault();
        }

        public AssemblyVersion GetVersion(string version)
        {
            return this.Versions?.FirstOrDefault(x => x.Version == version);
        }



    }
}