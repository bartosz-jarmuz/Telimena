using System.Collections.Generic;

namespace Telimena.WebApp.Core.DTO
{
    public class UsageDataTableResult
    {
        public IEnumerable<DataTableTelemetryData> UsageData { get; set; } = new List<DataTableTelemetryData>();

        public int TotalCount { get; set; }
        public int FilteredCount { get; set; }


    }
}