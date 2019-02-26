using System;
using System.Collections.Generic;

namespace Telimena.WebApp.Core.Models
{
    public abstract class TelemetryDetail
    {
        //[Obsolete("For EF")]
        protected TelemetryDetail()
        {
        }

        protected TelemetryDetail(Guid id)
        {
            this.Id = id;
        }

        public Guid Id { get; set; }
        public string Sequence { get; set; }
        public string IpAddress { get; set; }
        public string AssemblyVersion { get; set; }
        public string FileVersion { get; set; }
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
        public int TelemetrySummaryId { get; set; }

        public string EntryKey { get; set; }

        public string UserId { get; set; }

        public abstract TelemetrySummary GetTelemetrySummary();
        public abstract IReadOnlyList<TelemetryUnit> GetTelemetryUnits();
    }
}