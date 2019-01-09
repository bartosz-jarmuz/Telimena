using System.Threading.Tasks;
using TelimenaClient.Serializer;

namespace TelimenaClient
{
    internal class SerializingPipelineProcessor : ITelemetryPipelineProcessor
    {
        public Task<TelemetryPipelineTransportUnit> Process(TelemetryPipelineTransportUnit unit)
        {
            TelimenaSerializer serializer = new TelimenaSerializer();
            string stringified = serializer.SerializeTelemetryItem(unit.TelemetryItem);
            unit.SerializedEntry = stringified;
            return Task.FromResult(unit);
        }
    }
}