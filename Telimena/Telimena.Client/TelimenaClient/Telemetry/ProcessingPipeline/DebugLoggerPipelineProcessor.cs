using System.Diagnostics;
using System.Threading.Tasks;

namespace TelimenaClient
{
    internal class DebugLoggerPipelineProcessor : ITelemetryPipelineProcessor
    {
        public Task<TelemetryPipelineTransportUnit> Process(TelemetryPipelineTransportUnit unit)
        {
            if (unit.SerializedEntry != null)
            {
                Debug.Indent();
                Debug.WriteLine("Telimena telemetry: " + unit.SerializedEntry);
                Debug.Unindent();
                Debug.Flush();
            }
            return Task.FromResult(unit);
        }
    }
}