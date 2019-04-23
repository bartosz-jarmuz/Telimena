namespace Telimena.WebApp.Core.Models.Telemetry
{
    public class ExceptionTelemetryUnit : TelemetryUnit
    {
        public virtual ExceptionInfo ExceptionInfo { get; set; }
    }
}