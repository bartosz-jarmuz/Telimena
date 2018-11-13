using System.ComponentModel.DataAnnotations.Schema;

namespace Telimena.WebApp.Core.Models
{
    public class EventTelemetryUnit : TelemetryUnit
    {

        public virtual  EventTelemetryDetail EventTelemetryDetail { get; set; }
    }
}