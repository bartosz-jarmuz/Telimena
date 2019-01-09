namespace TelimenaClient
{
    /// <inheritdoc />
    public class TelemetryModule : ITelemetryModule
    {

        /// <summary>
        /// Creates new instance
        /// </summary>
        /// <param name="telimena"></param>
        public TelemetryModule(Telimena telimena)
        {
            TelemetryProcessingPipeline pipeline = BuildProcessingPipeline(telimena.Locator);
            this.Async = new Telimena.AsyncTelemetryHandler(telimena, pipeline);
            this.Blocking = new Telimena.BlockingTelemetryHandler(telimena);
        }

        internal static TelemetryProcessingPipeline BuildProcessingPipeline(Locator locator)
        {
            var telemetryProcessingPipeline = new TelemetryProcessingPipeline();
            telemetryProcessingPipeline.Register(new SerializingPipelineProcessor());
            telemetryProcessingPipeline.Register(new DebugLoggerPipelineProcessor());
            telemetryProcessingPipeline.Register(new PersistingPipelineProcessor(locator.TelemetryStorageDirectory));
            return telemetryProcessingPipeline;
        }


        /// <inheritdoc />
        public IAsyncTelemetryHandler Async { get; }


        /// <inheritdoc />
        public IBlockingTelemetryHandler Blocking { get; }

    }
}