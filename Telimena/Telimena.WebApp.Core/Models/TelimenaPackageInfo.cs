namespace Telimena.WebApp.Core.Models
{
    public class TelimenaPackageInfo : RepositoryFileBase, IRepositoryFile
    {
        public const string UpdaterFileName = "Telimena.Client.zip";
        protected TelimenaPackageInfo() : base() { }
        public TelimenaPackageInfo(string version, long fileSizeBytes) : base(UpdaterFileName, fileSizeBytes)
        {
            this.Version = version;
        }

        public int Id { get; set; }
        public string Version { get; set; }
    }
}