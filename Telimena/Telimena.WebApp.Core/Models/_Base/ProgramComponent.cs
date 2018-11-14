using System;
using System.Collections.Generic;
using System.Linq;

namespace Telimena.WebApp.Core.Models
{
    public abstract class ProgramComponent
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual Program Program { get; set; }
        public int ProgramId { get; set; }
        public DateTime FirstReportedDate { get; set; }

        public abstract IReadOnlyList<TelemetrySummary> GetTelemetrySummaries();

        public IReadOnlyList<TelemetryDetail> GetTelemetryDetails(int clientAppUserId)
        {
            TelemetrySummary summary = this.GetTelemetrySummary(clientAppUserId);
            return summary?.GetTelemetryDetails();
        }

        public TelemetrySummary GetTelemetrySummary(int clientAppUserId)
        {
            return this.GetTelemetrySummaries().FirstOrDefault(x => x.ClientAppUser.Id == clientAppUserId);
        }

        public abstract TelemetrySummary AddTelemetrySummary(int clientAppUserId);
    }
}