using System;

namespace Telimena.WebApp.Core.DTO
{
    public class UsageData
    {
        public DateTimeOffset DateTime { get; set; }

        public string CustomData { get; set; }

        public string UserName { get; set; }
        public string ViewName { get; set; }
        public string ProgramVersion { get; set; }
    }
}