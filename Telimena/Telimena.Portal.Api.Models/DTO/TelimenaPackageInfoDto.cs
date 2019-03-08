using System;

namespace Telimena.Portal.Api.Models.DTO
{
    public class TelimenaPackageInfoDto
    {
        public Guid Id { get; set; }

        public DateTimeOffset UploadedDate { get; set; }

        public long FileSizeBytes { get; set; }
    }

    public class ProgramPackageInfoDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string SupportedToolkitVersion { get; set; }
       // public string Version { get; set; }

        public long FileSizeBytes { get; set; }
        public DateTimeOffset UploadedDate { get; set; }
    }

    public class ProgramUpdatePackageInfoDto
    {
        public Guid Id { get; set; }
        public DateTimeOffset UploadedDate { get; set; }
        public string SupportedToolkitVersion { get; set; }
        public string Version { get; set; }
        public bool IsBeta { get; set; }
        public string FileName { get; set; }

        public long FileSizeBytes { get; set; }
        public string ReleaseNotes { get; set; }
    }
}