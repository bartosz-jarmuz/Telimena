namespace Telimena.WebApp.Core.DTO
{
    /// <summary>
    /// Type of telemetry item 
    /// </summary>
    public enum TelemetryItemTypes
    {
        /// <summary>
        /// An event
        /// </summary>
        Event,
        /// <summary>
        /// A view
        /// </summary>
        View,
        /// <summary>
        /// The log message
        /// </summary>
        LogMessage,
        /// <summary>
        /// The exception
        /// </summary>
        Exception,
        /// <summary>
        /// The metric
        /// </summary>
        Metric,
        /// <summary>
        /// The heartbeat
        /// </summary>
        Heartbeat


    }
}