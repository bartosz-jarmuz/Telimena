using System.ComponentModel.DataAnnotations.Schema;

namespace Telimena.WebApp.Core.Models
{

    public class ViewTelemetryUnit : TelemetryUnit
    {
        public virtual ViewTelemetryDetail TelemetryDetail { get; set; }
    }
}