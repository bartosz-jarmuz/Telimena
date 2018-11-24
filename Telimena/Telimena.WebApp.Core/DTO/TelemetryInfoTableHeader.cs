namespace Telimena.WebApp.Core.DTO
{
    public class TelemetryInfoTableHeader
    {
        public TelemetryInfoHeaderItem TelemetryDetailId { get; set; } = new TelemetryInfoHeaderItem();
        public TelemetryInfoHeaderItem Date { get; set; } = new TelemetryInfoHeaderItem("date");
        public TelemetryInfoHeaderItem Time { get; set; } = new TelemetryInfoHeaderItem("time");
        public TelemetryInfoHeaderItem ComponentName { get; set; } = new TelemetryInfoHeaderItem();
        public TelemetryInfoHeaderItem UserName { get; set; } = new TelemetryInfoHeaderItem();
        public TelemetryInfoHeaderItem Key { get; set; } = new TelemetryInfoHeaderItem();
        public TelemetryInfoHeaderItem Value { get; set; } = new TelemetryInfoHeaderItem();
    }
}