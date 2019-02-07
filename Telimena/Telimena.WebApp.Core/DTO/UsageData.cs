using System;

namespace Telimena.WebApp.Core.DTO
{
    public class UsageData
    {
        public DateTimeOffset Timestamp { get; set; }
        public string UserName { get; set; }
        public string EntryKey { get; set; }
        public string ProgramVersion { get; set; }
        public string Sequence { get; set; }
        public string Details { get; set; }
    }
}