using System;
using System.ComponentModel.DataAnnotations.Schema;
using Telimena.WebApp.Core.DTO.MappableToClient;

namespace Telimena.WebApp.Core.Models.Portal
{
    public class TelimenaPackageInfo : RepositoryFileBase, IRepositoryFile
    {
        protected TelimenaPackageInfo()
        {
        }

        public TelimenaPackageInfo(string version, long fileSizeBytes) : base(DefaultToolkitNames.ZippedPackageName, fileSizeBytes)
        {
            this.Version = version;
        }

        
        [ForeignKey(nameof(TelimenaToolkitData))]
        public int Id { get; set; }  

        [Index(IsUnique = true, IsClustered =  false)]
        public Guid PublicId { get; set; } = Guid.NewGuid();

        public string Version { get; set; }
        public bool IsBeta { get; set; }
        public bool IntroducesBreakingChanges { get; set; }
        public virtual TelimenaToolkitData TelimenaToolkitData { get; set; }

        public void UpdateContentAndMetadata(bool isBeta, bool introducesBreakingChanges, string version, long fileSizeBytes)
        {
            this.IsBeta = isBeta;
            this.IntroducesBreakingChanges = introducesBreakingChanges;
            this.Version = version;
            this.FileSizeBytes = fileSizeBytes;
            this.UploadedDate = DateTimeOffset.UtcNow;
        }

    }
}