namespace TelimenaClient
{
    /// <summary>
    /// A collection of Telemetry related functions
    /// </summary>
    public interface ITelemetryModule : IFluentInterface
    {
        /// <summary>
        /// Asynchronous telemetry methods. All calls should be awaited
        /// </summary>
        IAsyncTelemetryHandler Async { get; }

        /// <summary>
        /// Synchronous wrappers around async telemetry methods
        /// </summary>
        IBlockingTelemetryHandler Blocking { get; }
    }
}