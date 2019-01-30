using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TelimenaClient
{
    /// <summary>
    /// A collection of Track related functions
    /// </summary>
    public interface ITelemetryModule : IFluentInterface
    {
        /// <summary>
        ///     Reports an occurence of an event
        /// </summary>
        /// <param name="eventName">Name of the event</param>
        /// <param name="telemetryData">Custom telemetry data</param>
        /// <returns></returns>
        void Event(string eventName, Dictionary<string, string> telemetryData = null);

        /// <summary>
        ///     Report the usage of the application view.
        /// </summary>
        /// <param name="viewName">The name of the view. If left blank, it will report the name of the invoked method</param>
        /// <param name="telemetryData"></param>
        /// <returns></returns>
        void View(string viewName, Dictionary<string, object> telemetryData = null);

        /// <summary>
        /// Tracks the occurence of the given exception
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="telemetryData">The telemetry data.</param>
        void Exception(Exception exception, Dictionary<string, object> telemetryData = null);

        /// <summary>
        /// Sends all the accumulated data now.
        /// </summary>
        void SendAllDataNow();

        /// <summary>
        /// Initializes the telemetry client.
        /// </summary>
        void InitializeTelemetryClient();
    }
}
