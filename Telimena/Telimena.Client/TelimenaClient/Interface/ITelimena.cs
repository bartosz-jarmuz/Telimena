namespace TelimenaClient
{
    /// <summary>
    ///     Telemetry and Lifecycle Management Engine App
    ///     <para>This is a client SDK that allows handling application telemetry and lifecycle</para>
    /// </summary>
    public interface ITelimena : IFluentInterface
    {
        /// <summary>
        /// Application updating related methods
        /// </summary>
        IUpdatesModule Updates { get; }

        /// <summary>
        /// Telemetry related methods
        /// </summary>
        ITelemetryModule Telemetry { get; }
       
        /// <summary>
        /// Telimena Client properties
        /// </summary>
        ITelimenaProperties Properties { get; }
    }
}