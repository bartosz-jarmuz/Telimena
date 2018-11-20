using System;

namespace Telimena.WebApp.Core.DTO
{
    public class ProgramSummary
    {
        public string ProgramName { get; set; }
        public string DeveloperName { get; set; }
        public Guid TelemetryKey { get; set; }

        public string LatestVersion { get; set; }
        public string AssociatedToolkitVersion { get; set; }

        public int UsersCount { get; set; }

        public int ViewsCount { get; set; }
        public int TotalViewsUsageCount { get; set; }

        public int TotalUsageCount { get; set; }
        public int TodayUsageCount { get; set; }

        public DateTime? LastUsage { get; set; }

        public DateTime RegisteredDate { get; set; }
        public int TotalTodayViewsUsageCount { get; set; }
    }
}