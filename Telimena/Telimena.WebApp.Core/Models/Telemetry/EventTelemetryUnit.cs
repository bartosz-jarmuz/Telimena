namespace Telimena.WebApp.Core.Models.Telemetry
{
    public class EventTelemetryUnit : TelemetryUnit
    {
        public virtual  EventTelemetryDetail TelemetryDetail { get; set; }
    }
}