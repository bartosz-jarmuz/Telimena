namespace Telimena.WebApp.Core.Models
{
    public class UpdaterPackageInfo : RepositoryFileBase, IRepositoryFile
    {
        protected UpdaterPackageInfo()
        {
        }

        public UpdaterPackageInfo(string version, long fileSizeBytes, string minimumRequiredToolkitVersion) : base(UpdaterPackageName, fileSizeBytes)
        {
            this.Version = version;
            this.MinimumRequiredToolkitVersion = minimumRequiredToolkitVersion;
        }

        public const string UpdaterPackageName = "Updater.zip";
        public const string UpdaterFileName = "Updater.exe";

        public int Id { get; set; }
        public string Version { get; set; }

        public string MinimumRequiredToolkitVersion { get; set; }

        public bool IsBeta { get; set; }
    }
}