using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights.DataContracts;
using TelimenaClient.Model;

namespace TelimenaClient
{
    /// <summary>
    /// A collection of Tracking related functions
    /// </summary>
    public interface ITelemetryModule : IFluentInterface
    {
        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="level">The verbosity level</param>
        /// <param name="message">The message.</param>
        void Log(LogLevel level, string message);

        /// <summary>
        ///     Reports an occurence of an event
        /// </summary>
        /// <param name="eventName">Name of the event</param>
        /// <param name="telemetryData">Custom telemetry data</param>
        /// <param name="metrics">Event related metrics</param>
        /// <returns></returns>
        void Event(string eventName, Dictionary<string, string> telemetryData = null, Dictionary<string, double> metrics = null);

        /// <summary>
        ///     Report the usage of the application view.
        /// </summary>
        /// <param name="viewName">The name of the view. If left blank, it will report the name of the invoked method</param>
        /// <param name="telemetryData"></param>
        /// <param name="metrics">View access related metrics</param>
        /// <returns></returns>
        void View(string viewName, Dictionary<string, string> telemetryData = null, Dictionary<string, double> metrics = null);

        /// <summary>
        /// Tracks the occurence of the given exception
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="telemetryData">The telemetry data.</param>
        void Exception(Exception exception, Dictionary<string, string> telemetryData = null);

        /// <summary>
        /// Sends all the accumulated data now.
        /// </summary>
        void SendAllDataNow();

    }
}
