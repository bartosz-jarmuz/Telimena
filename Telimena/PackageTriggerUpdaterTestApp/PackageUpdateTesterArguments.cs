using System;
using TelimenaClient;

namespace PackageTriggerUpdaterTestApp
{
    public class PackageUpdateTesterArguments
    {
        public string ApiUrl { get; set; }

        public ProgramInfo ProgramInfo { get; set; }
        public string UserName { get; set; }

        public string ViewName { get; set; }

        public Guid TelemetryKey { get; set; }
    }
}