using System;

namespace Telimena.WebApp.Models.ProgramStatistics
{
    public class SequenceHistoryViewModel
    {
        public string ProgramName { get; set; }
        public Guid TelemetryKey { get; set; }

        public string SequenceId { get; set; }
    }
}