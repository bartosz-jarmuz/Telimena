using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DotNetLittleHelpers;
using Telimena.WebApp.Core.DTO.MappableToClient;

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
        public virtual RestrictedAccessList<AssemblyVersionInfo> Versions { get; set; } = new RestrictedAccessList<AssemblyVersionInfo>();

        public string GetFileName()
        {
            return this.Name + this.Extension;
        }
        public virtual AssemblyVersionInfo LatestVersion { get; private set; }

        public void AddVersion(VersionData version)
        {
            var existingOne = this.GetVersion(version);
            if (existingOne == null)
            {
                ((List<AssemblyVersionInfo>) this.Versions).Add(new AssemblyVersionInfo(version));
            }
        }

        public AssemblyVersionInfo GetVersion(VersionData version)
        {
            IEnumerable<AssemblyVersionInfo> matches = this.Versions.Where(x => x.AssemblyVersion == version.AssemblyVersion);

            if (!string.IsNullOrEmpty(version.FileVersion))
            {
                matches = matches.Where(x => x.FileVersion == version.FileVersion);
            }
            return matches.FirstOrDefault();
        }

        public AssemblyVersionInfo GetLatestVersion()
        {
            return this.Versions?.OrderByDescending(x => x.AssemblyVersion, new VersionStringComparer())?.FirstOrDefault();
        }

    }
}