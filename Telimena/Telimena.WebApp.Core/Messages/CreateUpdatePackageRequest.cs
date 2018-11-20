using System;

namespace Telimena.WebApp.Core.Messages
{
    public class CreateUpdatePackageRequest
    {
        public Guid TelemetryKey { get; set; }
        public string PackageVersion { get; set; }
        public string ToolkitVersionUsed { get; set; }
        public bool IsBeta { get; set; }
    }
}