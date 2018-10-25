using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DotNetLittleHelpers;

namespace Telimena.WebApp.Core.Models
{
    public class ProgramAssembly
    {
        public int Id { get; set; }
        public int ProgramId { get; set; }
        public virtual Program Program { get; set; }
        public virtual Program PrimaryOf { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Product { get; set; }
        public string Trademark { get; set; }
        public string Description { get; set; }
        public string Copyright { get; set; }
        public string Title { get; set; }
        public string Company { get; set; }
        public string FullName { get; set; }
        public virtual RestrictedAccessCollection<AssemblyVersion> Versions { get; set; } = new List<AssemblyVersion>();

        public string GetFileName()
        {
            return this.Name + this.Extension;
        }
        public virtual AssemblyVersion LatestVersion { get; private set; }

        public void AddVersion(string version)
        {
            if (this.Versions.All(x => x.Version != version))
            {
                ((Collection<AssemblyVersion>) this.Versions).Add(new AssemblyVersion {Version = version});
            }
        }

        public AssemblyVersion GetVersion(string version)
        {
            return this.Versions.FirstOrDefault(x => x.Version == version);
        }

        public AssemblyVersion GetLatestVersion()
        {
            return this.Versions?.OrderByDescending(x => x.Version, new VersionStringComparer())?.FirstOrDefault();
        }

    }
}