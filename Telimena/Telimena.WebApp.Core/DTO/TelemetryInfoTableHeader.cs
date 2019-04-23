namespace Telimena.WebApp.Core.DTO
{
    public class TelemetryInfoTableHeader
    {
        public TelemetryInfoHeaderItem Date { get; set; } = new TelemetryInfoHeaderItem("datetime");
        public TelemetryInfoHeaderItem Sequence { get; set; } = new TelemetryInfoHeaderItem();
        public TelemetryInfoHeaderItem ComponentName { get; set; } = new TelemetryInfoHeaderItem();
        public TelemetryInfoHeaderItem UserName { get; set; } = new TelemetryInfoHeaderItem();
        public TelemetryInfoHeaderItem Key { get; set; } = new TelemetryInfoHeaderItem();
        public TelemetryInfoHeaderItem PropertyValue { get; set; } = new TelemetryInfoHeaderItem();
        public TelemetryInfoHeaderItem MetricValue { get; set; } = new TelemetryInfoHeaderItem("number");
    }
}