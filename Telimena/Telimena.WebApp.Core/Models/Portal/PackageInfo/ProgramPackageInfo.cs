using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telimena.WebApp.Core.Models.Portal
{
    public class ProgramPackageInfo : RepositoryFileBase, IRepositoryFile
    {
        protected ProgramPackageInfo()
        {
        }

        public ProgramPackageInfo(string fileName, int programId, string version, long fileSizeBytes, string supportedToolkitVersion) : base(fileName, fileSizeBytes)
        {
            this.ProgramId = programId;
            //this.Version = version;
            this.SupportedToolkitVersion = supportedToolkitVersion;
        }

        [Key]
        public int Id { get; set; }

        [Index(IsUnique = true, IsClustered = false)]
        public Guid PublicId { get; set; } = Guid.NewGuid();

        public int ProgramId { get; set; }
        public string SupportedToolkitVersion { get; set; }

        public string Version { get; set; }
    }
}