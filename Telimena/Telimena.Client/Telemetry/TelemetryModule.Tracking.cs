using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using TelimenaClient.Model;

namespace TelimenaClient
{
    /// <inheritdoc />
    public partial class TelemetryModule : ITelemetryModule
    {
     

        /// <inheritdoc />
        public void View(string viewName, Dictionary<string, string> telemetryData = null, Dictionary<string, double> metrics = null)
        {
            this.TelemetryClient.TrackPageView(viewName);
        }

        /// <inheritdoc />
        public void Event(string eventName, Dictionary<string, string> telemetryData = null, Dictionary<string, double> metrics = null)
        {
            this.TelemetryClient.TrackEvent(eventName, telemetryData, metrics);
        }

        /// <inheritdoc />
        public void Exception(Exception exception, Dictionary<string, string> telemetryData = null)
        {
            this.TelemetryClient.TrackException(exception);
        }

        /// <inheritdoc />
        public void Log(LogLevel level,  string message)
        {
            this.TelemetryClient.TrackTrace(message, LogSeverityMapper.Map(level));
        }
    }
}