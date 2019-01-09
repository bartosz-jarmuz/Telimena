using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelimenaClient
{
    //each event processing order
    //1. serialize ITelemetry
    //2. log serialized to debug
    //3. log serialized to ETW?
    //4. compress serialized 
    //5. save compressed to file

    internal class TelemetryProcessingPipeline
    {
        private TelemetryPipelineTransportUnit transportUnit;

        public void Register(ITelemetryPipelineProcessor processor)
        {
            this.Processors.Add(processor);
        }

        private List<ITelemetryPipelineProcessor> Processors { get; set; }  = new List<ITelemetryPipelineProcessor>();

        public async Task Process(TelemetryItem item)
        {
            this.transportUnit = new TelemetryPipelineTransportUnit(item);

            foreach (ITelemetryPipelineProcessor telemetryPipelineProcessor in this.Processors)
            {
                this.transportUnit = await  telemetryPipelineProcessor.Process(this.transportUnit);
            }
        }
    }
}
