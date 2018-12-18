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
            this.Async = new Telimena.AsyncTelemetryHandler(telimena);
            this.Blocking = new Telimena.BlockingTelemetryHandler(telimena);
        }


        /// <inheritdoc />
        public IAsyncTelemetryHandler Async { get; }


        /// <inheritdoc />
        public IBlockingTelemetryHandler Blocking { get; }

    }
}