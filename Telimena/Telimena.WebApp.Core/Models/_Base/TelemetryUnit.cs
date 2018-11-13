using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telimena.WebApp.Core.Models
{
    public abstract class TelemetryUnit
    {
        public int Id { get; set; }

        [MaxLength(50)]
        public string Key { get; set; }

        public string Value { get; set; }
    }
}