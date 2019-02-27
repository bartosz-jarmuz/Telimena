using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telimena.WebApp.Core.Models.Telemetry
{
    public class TelemetryRootObject
    {
        /// <summary>
        /// Purely as a PK in this table
        /// </summary>
        [Key]
        public int RootObjectId { get; set; }

        /// <summary>
        /// Corresponds to PK on Programs table 
        /// </summary>
        [Index(IsUnique = true)]
        public int ProgramId { get; set; }

        /// <summary>
        /// The Telemetry key
        /// </summary>
        [Index(IsUnique = true, IsClustered = false)]
        public Guid TelemetryKey { get; set; }

        public virtual ICollection<View> Views { get; set; } = new List<View>();

        public virtual ICollection<Event> Events { get; set; } = new List<Event>();
    }
}