using System.Collections.Generic;

namespace Telimena.WebApp.Core.Models
{
    public interface ITelemetryAware
    {
        int Id { get; set; }
        string Name { get; set; }
        Program Program { get; set; }
        IReadOnlyList<TelemetrySummary> GetTelemetrySummaries();
        IReadOnlyList<TelemetryDetail> GetTelemetryDetails(int clientAppUserId);
        TelemetrySummary GetTelemetrySummary(int clientAppUserId);
        TelemetrySummary AddTelemetrySummary(int clientAppUserId);
    }
}