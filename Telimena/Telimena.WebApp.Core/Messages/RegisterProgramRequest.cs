using System;

namespace Telimena.WebApp.Core.Messages
{
    public class RegisterProgramRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string PrimaryAssemblyFileName { get; set; }
        public Guid TelemetryKey { get; set; }
    }
}