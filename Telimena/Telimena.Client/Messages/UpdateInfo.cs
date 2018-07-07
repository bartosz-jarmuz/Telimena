namespace Telimena.Client
{
    using System.Collections.Generic;

    public class UpdateInfo 
    {
        public VersionInfo PrimaryAssemblyVersion { get; set; }
        public ICollection<VersionInfo> HelperAssemblyVersions{ get; set; }
        public string LatestTelimenaVersion { get; set; }
        public bool IsTelimenaVersionBeta { get; set; }
    }
}