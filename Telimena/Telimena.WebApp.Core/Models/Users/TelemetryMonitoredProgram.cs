using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Telimena.WebApp.Core.Models
{
    public class TelemetryMonitoredProgram
    {
        public int ProgramId { get; set; }

        [Key]
        public Guid TelemetryKey { get; set; }

        public virtual ICollection<View> Views { get; set; } = new List<View>();
        public virtual ICollection<Event> Events { get; set; } = new List<Event>();
    }
}