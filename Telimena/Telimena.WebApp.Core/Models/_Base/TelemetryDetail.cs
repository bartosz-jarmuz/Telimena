using System;
using System.Collections.Generic;

namespace Telimena.WebApp.Core.Models
{
    public abstract class TelemetryDetail
    {
        [Obsolete("For EF")]
        private TelemetryDetail()
        {
        }

        protected TelemetryDetail(Guid id)
        {
            this.Id = id;
        }

        public Guid Id { get; set; }
        public string IpAddress { get; set; }
        public virtual AssemblyVersionInfo AssemblyVersion { get; set; }
        public int? AssemblyVersionId { get; set; }
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
        public int TelemetrySummaryId { get; set; }

        public abstract TelemetrySummary GetTelemetrySummary();
        public abstract IReadOnlyList<TelemetryUnit> GetTelemetryUnits();
        public abstract void SetTelemetrySummary(TelemetrySummary summary);
    }
}