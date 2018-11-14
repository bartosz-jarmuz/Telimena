using System.Collections.Generic;

namespace Telimena.WebApp.Core.Models
{
    public interface ITelemetryAware
    {
        IReadOnlyList<TelemetrySummary> GetTelemetrySummaries();
        IReadOnlyList<TelemetryDetail> GetTelemetryDetails(int clientAppUserId);
        TelemetrySummary GetTelemetrySummary(int clientAppUserId);
        TelemetrySummary AddTelemetrySummary(int clientAppUserId);
    }
}