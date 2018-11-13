using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telimena.WebApp.Core.Models
{
    public abstract class TelemetryDetail
    {
        public int Id { get; set; }
        public string IpAddress { get; set; }
        public virtual AssemblyVersionInfo AssemblyVersion { get; set; }
        public int? AssemblyVersionId { get; set; }
        public DateTime DateTime { get; set; } = DateTime.UtcNow;
        public int UsageSummaryId { get; set; }

        [NotMapped]
        public abstract IEnumerable<TelemetryUnit> TelemetryUnits { get;  }

    }
}