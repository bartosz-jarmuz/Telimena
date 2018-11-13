using System.Collections.Generic;
using System.Linq;

namespace Telimena.WebApp.Core.Models
{
    public class View : ProgramComponent
    {
       // public virtual ICollection<ViewUsageSummary> UsageSummaries { get; set; } = new List<ViewUsageSummary>();

        public virtual ICollection<ViewTelemetrySummary> UsageSummaries { get; set; } = new List<ViewTelemetrySummary>();


        public ICollection<ViewTelemetryDetail> GetTelemetryDetails(int clientAppUserId)
        {
            ViewTelemetrySummary usage = this.GetTelemetrySummary(clientAppUserId);
            return usage.Details.Cast<ViewTelemetryDetail>().ToList();
        }

        public ViewTelemetrySummary GetTelemetrySummary(int clientAppUserId)
        {
            return this.UsageSummaries.FirstOrDefault(x => x.ClientAppUser.Id == clientAppUserId);
        }
    }
}