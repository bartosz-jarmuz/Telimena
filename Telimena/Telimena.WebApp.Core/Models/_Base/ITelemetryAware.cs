using System.Collections.Generic;

namespace Telimena.WebApp.Core.Models
{
    public interface ITelemetryAware
    {
        string Name { get; set; }
        IReadOnlyList<TelemetrySummary> GetTelemetrySummaries();
        IReadOnlyList<TelemetryDetail> GetTelemetryDetails(int clientAppUserId);
        TelemetrySummary GetTelemetrySummary(int clientAppUserId);
        TelemetrySummary AddTelemetrySummary(int clientAppUserId);
    }
}