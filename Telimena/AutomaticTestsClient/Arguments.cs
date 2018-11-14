using System;
using TelimenaClient;

namespace AutomaticTestsClient
{
    public class Arguments
    {
        public string ApiUrl { get; set; }

        public Actions Action { get; set; }

        public ProgramInfo ProgramInfo { get; set; }
        public string UserName { get; set; }

        public string ViewName { get; set; }

        public Guid TelemetryKey { get; set; }
    }
}