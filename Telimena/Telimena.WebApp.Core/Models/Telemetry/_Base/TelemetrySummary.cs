using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.DTO.MappableToClient;

namespace Telimena.WebApp.Core.Models.Telemetry
{
    public abstract class TelemetrySummary
    {
        [Key, Index(IsUnique = true, IsClustered = false)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [DatabaseGenerated(DatabaseGeneratedOption.Identity), Index(IsUnique = true, IsClustered = true)]
        public int ClusterId { get; set; }

        public DateTimeOffset LastTelemetryUpdateTimestamp { get; set; } = DateTimeOffset.UtcNow;

        public virtual ClientAppUser ClientAppUser { get; set; }
        public int? ClientAppUserId { get; set; }

        private int summaryCount;

        public int SummaryCount
        {
            get
            {
                this.summaryCount = this.GetTelemetryDetails()?.Count() ?? 0;
                return this.summaryCount;
            }
            set => this.summaryCount = value;
        }

        public abstract List<TelemetryDetail> GetTelemetryDetails();
        public abstract ITelemetryAware GetComponent();

        public abstract void AddTelemetryDetail(string ipAddress, VersionData versionInfo, TelemetryItem telemetryItem);

        public virtual void UpdateTelemetry(VersionData versionInfo, string ipAddress, TelemetryItem telemetryItem)
        {
            this.LastTelemetryUpdateTimestamp = DateTimeOffset.UtcNow;
            this.AddTelemetryDetail(ipAddress, versionInfo, telemetryItem);
        }
    }
}