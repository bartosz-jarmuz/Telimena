using System;

namespace Telimena.WebApp.Core.DTO
{
    public class VersionInfo
    {
        public string AssemblyName { get; set; }
        public Guid AssemblyId { get; set; }
        public string LatestVersion { get; set; }
        public Guid LatestVersionId { get; set; }
    }
}