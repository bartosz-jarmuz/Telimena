using System;
using System.Collections.Generic;

namespace Telimena.WebApp.Core.Models.Telemetry
{
    public interface ITelemetryAware
    {
        Guid Id { get; set; }
        int ClusterId { get; set; }
        string Name { get; set; }
        TelemetryRootObject Program { get; set; }
        DateTimeOffset FirstReportedDate { get; set; }
        IReadOnlyList<TelemetrySummary> GetTelemetrySummaries();
        IReadOnlyList<TelemetryDetail> GetTelemetryDetails(int clientAppUserId);
        TelemetrySummary GetTelemetrySummary(int clientAppUserId);
        TelemetrySummary AddTelemetrySummary(int clientAppUserId);
    }
}