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

        public ProgramPackageInfo(string fileName, int programId, long fileSizeBytes, string supportedToolkitVersion) : base(fileName, fileSizeBytes)
        {
            this.ProgramId = programId;
            this.SupportedToolkitVersion = supportedToolkitVersion;
        }

        [Key]
        public int Id { get; set; }

        [Index(IsUnique = true, IsClustered = false)]
        public Guid PublicId { get; set; }

        public int ProgramId { get; set; }
        public string SupportedToolkitVersion { get; set; }
    }
}