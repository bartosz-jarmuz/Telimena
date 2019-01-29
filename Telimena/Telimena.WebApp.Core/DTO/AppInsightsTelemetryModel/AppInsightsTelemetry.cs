using System;
// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace Telimena.WebApp.Core.DTO.AppInsightsTelemetryModel
{

    public class AppInsightsTelemetry
    {
        public string name { get; set; }
        public DateTime time { get; set; }
        public string seq { get; set; }
        public Tags tags { get; set; }
        public Data data { get; set; }
    }
}
