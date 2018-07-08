namespace Telimena.Client.Model
{
    public class AssemblyUpdateInfo
    {
        public AssemblyUpdateInfo(AssemblyInfo assemblyInfo, VersionInfo latestVersionInfo)
        {
            this.AssemblyInfo = assemblyInfo;
            this.LatestVersionInfo = latestVersionInfo;
            this.AssemblyName = assemblyInfo.Name;
            this.IsBeta = latestVersionInfo.IsBeta;
        }

        public AssemblyInfo AssemblyInfo { get; }
        public VersionInfo LatestVersionInfo { get; }
        public string AssemblyName { get; set; }
        public bool IsBeta { get; set; }
    }
}