using System.Collections.Generic;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Portal;

namespace Telimena.WebApp.Core.DTO
{
    public class AllProgramsSummaryData
    {
        public int TotalProgramsCount { get; set; }
        public int TotalViewsCount { get; set; }
        public int TotalViewsUsageCount { get; set; }
        public Program NewestProgram { get; set; }
        public int TotalAppUsersCount { get; set; }
        public int AppUsersRegisteredLast7DaysCount { get; set; }
        public int TotalEventsCount { get; set; }
        public int TotalEventUsageCount { get; set; }
        public int TotalExceptionsInLast7Days { get; set; }
        public string MostPopularExceptionInLast7Days { get; set; }
    }
}