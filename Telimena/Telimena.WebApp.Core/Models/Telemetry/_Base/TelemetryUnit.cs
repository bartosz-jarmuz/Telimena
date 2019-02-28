using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telimena.WebApp.Core.Models.Telemetry
{
    public abstract class TelemetryUnit
    {
        [Key, Index(IsUnique = true, IsClustered = false)]
        public Guid Id { get; set; } = Guid.NewGuid();
        [DatabaseGenerated(DatabaseGeneratedOption.Identity), Index(IsUnique = true, IsClustered = true)]
        public int ClusterId { get; set; }

        [MaxLength(50)]
        public string Key { get; set; }

        public string ValueString { get; set; }

        public double ValueDouble { get; set; }
    }
}