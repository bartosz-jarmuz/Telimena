﻿using System;
using System.Collections.Generic;

namespace Telimena.WebApp.Core.Models
{
    public interface ITelemetryAware
    {
        int Id { get; set; }
        string Name { get; set; }
        TelemetryMonitoredProgram Program { get; set; }
        DateTimeOffset FirstReportedDate { get; set; }
        IReadOnlyList<TelemetrySummary> GetTelemetrySummaries();
        IReadOnlyList<TelemetryDetail> GetTelemetryDetails(int clientAppUserId);
        TelemetrySummary GetTelemetrySummary(int clientAppUserId);
        TelemetrySummary AddTelemetrySummary(int clientAppUserId);
    }
}