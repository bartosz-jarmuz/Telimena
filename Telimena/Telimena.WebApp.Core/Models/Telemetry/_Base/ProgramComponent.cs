using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Telimena.WebApp.Core.Models.Telemetry
{
    public abstract class ProgramComponent : ITelemetryAware
    {
        [Key,Index(IsUnique = true, IsClustered = false)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [DatabaseGenerated(DatabaseGeneratedOption.Identity), Index(IsUnique = true, IsClustered = true)]
        public int ClusterId { get; set; }
        public string Name { get; set; }
        public virtual TelemetryRootObject Program { get; set; }
        public int ProgramId { get; set; }
        public DateTimeOffset FirstReportedDate { get; set; } = DateTimeOffset.UtcNow;

        public abstract IReadOnlyList<TelemetrySummary> GetTelemetrySummaries();

        public IReadOnlyList<TelemetryDetail> GetTelemetryDetails(int clientAppUserId)
        {
            TelemetrySummary summary = this.GetTelemetrySummary(clientAppUserId);
            return summary?.GetTelemetryDetails();
        }

        public TelemetrySummary GetTelemetrySummary(int clientAppUserId)
        {
            return this.GetTelemetrySummaries().FirstOrDefault(x => x.ClientAppUserId == clientAppUserId);
        }

        public abstract TelemetrySummary AddTelemetrySummary(int clientAppUserId);
    }
}