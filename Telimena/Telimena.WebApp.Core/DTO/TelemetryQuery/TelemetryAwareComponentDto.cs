using System;
using System.Collections.Generic;
using Telimena.WebApp.Core.Messages;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Telemetry;

namespace Telimena.WebApp.Core.DTO
{
    public class TelemetryAwareComponentDto
    {
        [Obsolete("For serialization")]
        public TelemetryAwareComponentDto()
        {

        }

        public TelemetryAwareComponentDto(ITelemetryAware telemetryAwareComponent, List<string> propertiesToInclude)
        {
            if (propertiesToInclude.Contains(nameof(this.ComponentKey)))
            {
                this.ComponentKey = telemetryAwareComponent.Name;
            }
            if (propertiesToInclude.Contains(nameof(this.FirstReported)))
            {
                this.FirstReported = telemetryAwareComponent.FirstReportedDate;
            }
        }

        public string ComponentKey { get; set; }
       
        public List<TelemetrySummaryDto> Summaries { get; set;  } 
        public DateTimeOffset? FirstReported { get; set; }
    }
}