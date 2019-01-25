using System;
using System.Diagnostics;
using System.Timers;

namespace TelimenaClient
{
    internal sealed class TelemetryBroadcaster
    {
        public static TelemetryBroadcaster Instance => InstanceInternal.Value;

        private static readonly Lazy<TelemetryBroadcaster> InstanceInternal =
            new Lazy<TelemetryBroadcaster>(() => new TelemetryBroadcaster());

        private readonly Timer timer;
        private BroadcastingPipeline pipeline;

        // Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
        static TelemetryBroadcaster()
        {
        }

        private TelemetryBroadcaster()
        {
            //timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            pipeline.Process();
        }
         
        public void Initialize(BroadcastingPipeline pipeline)
        {
            this.pipeline = pipeline;
        }

    }
}