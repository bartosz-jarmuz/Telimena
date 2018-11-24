namespace Telimena.WebApp.Core.DTO
{
    public class TelemetryInfoTableHeader
    {
        public TelemetryInfoHeaderItem DateTime { get; set; } = new TelemetryInfoHeaderItem();
        public TelemetryInfoHeaderItem ComponentName { get; set; } = new TelemetryInfoHeaderItem();
        public TelemetryInfoHeaderItem UserName { get; set; } = new TelemetryInfoHeaderItem();
        public TelemetryInfoHeaderItem Key { get; set; } = new TelemetryInfoHeaderItem();
        public TelemetryInfoHeaderItem Value { get; set; } = new TelemetryInfoHeaderItem();
    }
}