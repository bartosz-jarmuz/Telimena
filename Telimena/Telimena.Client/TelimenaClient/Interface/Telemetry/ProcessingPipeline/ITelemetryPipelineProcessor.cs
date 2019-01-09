using System.Threading.Tasks;

namespace TelimenaClient
{
    /// <summary>
    /// Processes the telemetry unit
    /// </summary>
    internal interface ITelemetryPipelineProcessor
    {
        Task<TelemetryPipelineTransportUnit> Process(TelemetryPipelineTransportUnit unit);
    }
}