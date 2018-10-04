namespace Telimena.WebApp.Core.Models
{
    public class UpdaterPackageInfo : RepositoryFileBase, IRepositoryFile
    {
        protected UpdaterPackageInfo()
        {
        }

        public UpdaterPackageInfo(string version,string fileName, long fileSizeBytes, string minimumRequiredToolkitVersion) : base(fileName, fileSizeBytes)
        {
            this.Version = version;
            this.MinimumRequiredToolkitVersion = minimumRequiredToolkitVersion;
        }

        public int Id { get; set; }
        public string Version { get; set; }

        public string MinimumRequiredToolkitVersion { get; set; }

        public bool IsBeta { get; set; }

        public virtual Updater Updater { get; set; }

        public int UpdaterId { get; set; }
    }
}