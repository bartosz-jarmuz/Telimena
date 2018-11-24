// ReSharper disable InconsistentNaming
namespace Telimena.WebApp.Core.DTO
{
    public class TelemetryInfoHeaderItem
    {
        public TelemetryInfoHeaderItem()
        {
        }

        public TelemetryInfoHeaderItem(string type)
        {
            this.type = type;
        }

        public string type { get; set; } = "string";
    }
}