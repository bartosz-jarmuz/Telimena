using System;
using System.Collections.Generic;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Telemetry;

namespace Telimena.WebApp.Core.DTO
{
    public class TelemetryUnitDto
    {
        [Obsolete("For serialization")]
        public TelemetryUnitDto()
        {
        }

        public TelemetryUnitDto(TelemetryUnit telemetryUnit, List<string> propertiesToInclude)
        {
            if (propertiesToInclude.Contains(nameof(this.Key)))
            {
                this.Key = telemetryUnit.Key;
            }
            if (propertiesToInclude.Contains(nameof(this.ValueString)))
            {
                this.ValueString = telemetryUnit.ValueString;
            }
        }

        public string ValueString { get; set; }
        public string Key { get; set; }
    }
}