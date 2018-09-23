using System;

namespace Telimena.WebApp.Core.DTO
{
    public class UsageData
    {
        public DateTime DateTime { get; set; }

        public string CustomData { get; set; }

        public string UserName { get; set; }
        public string FunctionName { get; set; }
        public string ProgramVersion { get; set; }
    }
}