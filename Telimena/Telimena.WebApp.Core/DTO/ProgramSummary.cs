namespace Telimena.WebApp.Core.DTO
{
    using System;

    public class ProgramSummary
    {
        public string ProgramName { get; set; }
        public int ProgramId { get; set; }

        public string LatestVersion { get; set; }

        public int UsersCount { get; set; }

        public int FunctionsCount { get; set; }

        public int TotalUsageCount { get; set; }
        public int TodayUsageCount { get; set; }

        public DateTime LastUsage { get; set; }

        public DateTime RegisteredDate { get; set; }

    }
}