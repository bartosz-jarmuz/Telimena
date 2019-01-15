using System;
using System.Collections.Generic;
using Telimena.WebApp.Core.Models;

namespace Telimena.WebApp.Core.DTO
{
    public class TelemetryDetailDto
    {
        [Obsolete("For serialization")]
        public TelemetryDetailDto()
        {
        }

        public TelemetryDetailDto(TelemetryDetail telemetryDetail, List<string> propertiesToInclude)
        {
            if (propertiesToInclude.Contains(nameof(this.Timestamp)))
            {
                this.Timestamp = telemetryDetail.Timestamp;
            }
            if (propertiesToInclude.Contains(nameof(this.Ip)))
            {
                this.Ip = telemetryDetail.IpAddress;
            }
            if (propertiesToInclude.Contains(nameof(this.AssemblyVersion)))
            {
                this.AssemblyVersion = telemetryDetail.AssemblyVersion.AssemblyVersion;
            }
            if (propertiesToInclude.Contains(nameof(this.FileVersion)))
            {
                this.FileVersion = telemetryDetail.AssemblyVersion.FileVersion;
            }
        }

        public DateTimeOffset? Timestamp { get; set; }
        public List<TelemetryUnitDto> Units { get; set; }
        public string Ip { get; set; }
        public string AssemblyVersion { get; set; }
        public string FileVersion { get; set; }
    }
}