using System;

namespace Telimena.WebApp.Core.Messages
{
    public class RegisterProgramResponse
    {
        public RegisterProgramResponse(Guid telemetryKey, Guid developerId, string programPageUrl)
        {
            this.TelemetryKey = telemetryKey;
            this.DeveloperId = developerId;
            this.ProgramPageUrl = programPageUrl;
        }

        public RegisterProgramResponse(Exception exception)
        {
            this.Exception = exception;
        }

        public Guid TelemetryKey { get; set; }

        public string ProgramPageUrl { get; set; }
        public Guid DeveloperId { get; }

        public Exception Exception { get; set; }
    }
}