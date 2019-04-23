using System.Collections.Generic;
using Telimena.WebApp.Core.DTO;

namespace Telimena.WebApp.Core.Messages
{
    public class TelemetryQueryResponse
    {
        public List<TelemetryAwareComponentDto> TelemetryAware { get; set; } = new List<TelemetryAwareComponentDto>();

    }
}