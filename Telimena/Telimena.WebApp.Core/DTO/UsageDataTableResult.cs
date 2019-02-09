using System.Collections.Generic;

namespace Telimena.WebApp.Core.DTO
{
    public class UsageDataTableResult
    {
        public IEnumerable<TelemetryDataBase> UsageData { get; set; } = new List<TelemetryDataBase>();

        public int TotalCount { get; set; }
        public int FilteredCount { get; set; }


    }
}