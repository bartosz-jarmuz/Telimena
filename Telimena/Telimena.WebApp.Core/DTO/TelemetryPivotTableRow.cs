using System;

namespace Telimena.WebApp.Core.DTO
{
    public class TelemetryPivotTableRow
    {
        public Guid TelemetryDetailId { get; set; }
        public string Date { get; set; }
        public double Time { get; set; }
        public string UserName { get; set; }
        public string ComponentName { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}