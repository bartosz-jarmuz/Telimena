using System;

namespace Telimena.WebApp.Core.DTO
{
    public class AppUsersSummaryData
    {
        public string UserName { get; set; }
        public DateTimeOffset LastActiveDate { get; set; }
        public DateTimeOffset FirstSeenDate { get; set; }
        public int ActivityScore { get; set; }
        public string FileVersion { get; set; }
    }
}