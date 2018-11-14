using System;

namespace Telimena.WebApp.Core.Messages
{
    public class RegisterProgramResponse
    {
        public RegisterProgramResponse(int programId, Guid telemetryKey, int developerId)
        {
            this.ProgramId = programId;
            this.TelemetryKey = telemetryKey;
            this.DeveloperId = developerId;
        }

        public RegisterProgramResponse(Exception exception)
        {
            this.Exception = exception;
        }

        public Guid TelemetryKey { get; set; }

        public int ProgramId { get; set; }
        public int DeveloperId { get; }

        public Exception Exception { get; set; }
    }
}