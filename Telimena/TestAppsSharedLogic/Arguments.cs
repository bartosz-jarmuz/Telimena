using System;
using System.Text;
using TelimenaClient.Model;

namespace SharedLogic
{
    public class Arguments
    {
        public bool DebugMode { get; set; }

        public string ApiUrl { get; set; }

        public Actions Action { get; set; }

        public ProgramInfo ProgramInfo { get; set; }
        public string UserName { get; set; }

        public string ViewName { get; set; }

        public Guid TelemetryKey { get; set; }

    }
}