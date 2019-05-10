using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telimena.WebApp.Core.Models.Portal
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

        [Key]
        public int Id { get; set; }

        [Index(IsUnique = true, IsClustered = false)]
        public override Guid PublicId { get; set; } = Guid.NewGuid();
        public string Version { get; set; }
        public string MinimumRequiredToolkitVersion { get; set; }

        public bool IsBeta { get; set; }

        public virtual Updater Updater { get; set; }

        public int UpdaterId { get; set; }

        public void UpdateContentAndMetadata(long fileSizeBytes)
        {
            this.FileSizeBytes = fileSizeBytes;
            this.UploadedDate = DateTimeOffset.UtcNow;
        }
    }
}