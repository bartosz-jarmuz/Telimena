using System.Collections.Generic;

namespace Telimena.WebApp.Core.Models
{
    public class Event : ProgramComponent
    {
        public virtual ICollection<EventTelemetrySummary> TelemetrySummaries { get; set; } = new List<EventTelemetrySummary>();
    }
}