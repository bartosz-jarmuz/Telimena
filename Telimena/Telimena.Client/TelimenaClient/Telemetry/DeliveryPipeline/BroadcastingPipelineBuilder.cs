using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelimenaClient
{
    //delivery context would contain
    // - serialized units list
    // - associated files


    //delivery pipeline
    // get stored data from appdata
    // create request (?)
    // send the request
    // cleanup

    internal interface ITelemetryBroadcastingProcessor
    {
        Task Process(TelemetryBroadcastingContext context);
    }

    internal class BroadcastingPipelineBuilder
    {
        public void RegisterFactory(Func<ITelemetryBroadcastingProcessor> processorFactory)
        {
            this.Factories.Add(processorFactory);
        }

        private List<Func<ITelemetryBroadcastingProcessor>> Factories { get; set; } = new List<Func<ITelemetryBroadcastingProcessor>>();

        public BroadcastingPipeline Build()
        {
            var pipe = new BroadcastingPipeline();

            foreach (var processor in Factories)
            {
                pipe.Processors.Add(processor.Invoke());
            }

            return pipe;
        }
    }


    internal class BroadcastingPipeline
    {
        public List<ITelemetryBroadcastingProcessor> Processors { get; set; } = new List<ITelemetryBroadcastingProcessor>();

        public async Task Process()
        {
            var context = new TelemetryBroadcastingContext();
            foreach (ITelemetryBroadcastingProcessor telemetryPipelineProcessor in this.Processors)
            {
                await telemetryPipelineProcessor.Process(context).ConfigureAwait(false);
            }
        }
    }

   
}
