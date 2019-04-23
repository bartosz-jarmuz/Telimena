using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telimena.WebApp.Core.Models.Telemetry
{
    public abstract class TelemetryDetail
    {
        [Obsolete("For EF")]
        protected TelemetryDetail()
        {
        }

        protected TelemetryDetail(Guid id)
        {
            this.Id = id;
        }

        [Key, Index(IsUnique = true, IsClustered = false)]
        public Guid Id { get; set; } = Guid.NewGuid();
        [DatabaseGenerated(DatabaseGeneratedOption.Identity), Index(IsUnique = true, IsClustered = true)]
        public int ClusterId { get; set; }

        public string Sequence { get; set; }
        public string IpAddress { get; set; }
        public string AssemblyVersion { get; set; }
        public string FileVersion { get; set; }
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
        public Guid TelemetrySummaryId { get; set; }

        public string EntryKey { get; set; }

        public string UserIdentifier { get; set; }

        public abstract TelemetrySummary GetTelemetrySummary();
        public abstract IReadOnlyList<TelemetryUnit> GetTelemetryUnits();
    }
}