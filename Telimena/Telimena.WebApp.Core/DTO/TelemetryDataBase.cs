using System;
using System.Collections.Generic;

namespace Telimena.WebApp.Core.DTO
{
    public class TelemetryDataBase
    {
        public DateTimeOffset Timestamp { get; set; }
        public string UserName { get; set; }
        public string EntryKey { get; set; }
        public string ProgramVersion { get; set; }
        public string Sequence { get; set; }
        public string IpAddress { get; set; }

    }

    public class TelemetryData : TelemetryDataBase
    {
        public Dictionary<string, string> Values { get; set; } =new Dictionary<string, string>();
    }


    public class ExceptionData : TelemetryDataBase
    {
        public string ErrorMessage { get; set; }
        public List<TelemetryItem.ExceptionInfo.ParsedStackTrace> StackTrace { get; set; } = new List<TelemetryItem.ExceptionInfo.ParsedStackTrace>();
    }
}