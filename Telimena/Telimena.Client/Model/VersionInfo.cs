namespace Telimena.Client
{
    public class VersionInfo
    {
        public string AssemblyName { get; set; }
        public int AssemblyId { get; set; }
        public string LatestVersion { get; set; }
        public int LatestVersionId { get; set; }
        public bool IsBeta { get; set; }
    }
}