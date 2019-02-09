using System.Collections.Generic;

namespace Telimena.WebApp.Core.DTO
{
    public class UsageDataTableResult
    {
        public IEnumerable<UsageData> UsageData { get; set; } = new List<UsageData>();

        public int TotalCount { get; set; }
        public int FilteredCount { get; set; }


    }
}