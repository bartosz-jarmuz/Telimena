using System;
using System.Collections.Generic;

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

    public class ExceptionData : UsageData
    {
        public List<TelemetryItem.ExceptionInfo.ParsedStackTrace> StackTrace { get; set; } = new List<TelemetryItem.ExceptionInfo.ParsedStackTrace>();
    }
}