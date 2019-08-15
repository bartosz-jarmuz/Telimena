using System;

namespace Telimena.WebApp.Core.DTO
{
    public class ProgramSummary
    {
        public string ProgramName { get; set; }
        public string LatestVersion { get; set; }
        public string ToolkitVersion { get; set; }
        public string DeveloperName { get; set; }
        public Guid TelemetryKey { get; set; }
        public int UsersCount { get; set; }
        public DateTimeOffset? LastUpdateDate { get; set; }
        public DateTimeOffset RegisteredDate { get; set; }
        public int NumberOfUpdatePackages { get; set; }
    }

    public class ProgramUsageSummary
    {
        public string ProgramName { get; set; }
        public int ViewsCount { get; set; }
        public int TotalViewsUsageCount { get; set; }
        public int TotalTodayViewsUsageCount { get; set; }
        public int EventsCount { get; set; }
        public int TotalEventsUsageCount { get; set; }
        public int TotalTodayEventsUsageCount { get; set; }
        public int TotalUsageCount { get; set; }
        public int TodayUsageCount { get; set; }
        public DateTimeOffset? LastUsage { get; set; }
    }
}