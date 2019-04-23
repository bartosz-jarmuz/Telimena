using System;

namespace Telimena.Portal.Api.Models.DTO
{
    public class UpdaterPackageInfoDto
    {
        public Guid Id { get; set; }
        public string Version { get; set; }
        public string MinimumRequiredToolkitVersion { get; set; }
        public bool IsBeta { get; set; }
        public DateTimeOffset UploadedDate { get; set; } 
        public long FileSizeBytes { get; set; }

        public UpdaterDto Updater { get; set; }
    }
}
