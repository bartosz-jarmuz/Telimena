using System.Collections.Generic;

namespace Telimena.WebApp.Core.DTO
{
    public class TelemetryInfoTable
    {
        public IReadOnlyList<TelemetryPivotTableRow> Rows { get; set; }
        public TelemetryInfoTableHeader Header { get; set; }
    }
}