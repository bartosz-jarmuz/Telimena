using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telimena.WebApp.Core.Models
{
    public class TelimenaPackageInfo : RepositoryFileBase, IRepositoryFile
    {
        protected TelimenaPackageInfo()
        {
        }

        public TelimenaPackageInfo(string version, long fileSizeBytes) : base(ZippedPackageFileName, fileSizeBytes)
        {
            this.Version = version;
        }

        public const string ZippedPackageFileName = "Telimena.Client.zip";
        public const string TelimenaAssemblyName = "Telimena.Client.dll";

        [ForeignKey(nameof(TelimenaToolkitData))]
        public int Id { get; set; }

        public string Version { get; set; }
        public bool IsBeta { get; set; }
        public bool IntroducesBreakingChanges { get; set; }
        public virtual TelimenaToolkitData TelimenaToolkitData { get; set; }

        public void UpdateWithNewContent(bool isBeta, bool introducesBreakingChanges, string version, long fileSizeBytes)
        {
            this.IsBeta = isBeta;
            this.IntroducesBreakingChanges = introducesBreakingChanges;
            this.Version = version;
            this.FileSizeBytes = fileSizeBytes;
            this.UploadedDate = DateTime.UtcNow;
        }

    }
}