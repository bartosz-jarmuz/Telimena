using System;

namespace Telimena.WebApp.Core.Models
{
    public class UpdaterPackageInfo : RepositoryFileBase, IRepositoryFile
    {
        public const string UpdaterFileName = "Updater.zip";
        protected UpdaterPackageInfo() : base(){ }
        public UpdaterPackageInfo(string version, long fileSizeBytes, string minimumRequiredToolkitVersion) : base(UpdaterFileName, fileSizeBytes)
        {
            this.Version = version;
            this.MinimumRequiredToolkitVersion = minimumRequiredToolkitVersion;
        }

        public int Id { get; set; }
        public string Version { get; set; }

        public string MinimumRequiredToolkitVersion { get; set; }

        public bool IsBeta { get; set; }

    }
}
