#pragma warning disable 1591
using System;
using System.Collections.Generic;

namespace Telimena.WebApp.Models.ProgramStatistics
{
    public class ProgramStatisticsViewModel
    {
        public string ProgramName { get; set; }
        public Guid TelemetryKey { get; set; }

        public Dictionary<string, int> EventNames { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ViewsNames { get; set; } = new Dictionary<string, int>();
    }
}