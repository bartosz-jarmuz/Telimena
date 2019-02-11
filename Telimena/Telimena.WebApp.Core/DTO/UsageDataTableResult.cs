using System.Collections.Generic;

namespace Telimena.WebApp.Core.DTO
{
    public class UsageDataTableResult
    {
        public IEnumerable<DataTableTelemetryDataBase> UsageData { get; set; } = new List<DataTableTelemetryDataBase>();

        public int TotalCount { get; set; }
        public int FilteredCount { get; set; }


    }
}