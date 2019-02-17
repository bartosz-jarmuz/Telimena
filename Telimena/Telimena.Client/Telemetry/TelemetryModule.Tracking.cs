using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using TelimenaClient.Model;

namespace TelimenaClient
{
    /// <inheritdoc />
    public partial class TelemetryModule : ITelemetryModule
    {
     

        /// <inheritdoc />
        public void View(string viewName, Dictionary<string, string> telemetryData = null, Dictionary<string, double> metrics = null)
        {
            try
            {

                var tele = new PageViewTelemetry(viewName);
                if (telemetryData != null)
                {
                    Utils.CopyDictionary(telemetryData, tele.Properties);
                }

                this.TelemetryClient.TrackPageView(tele);
            }
            catch
            {
                if (!this.properties.SuppressAllErrors)
                {
                    throw;
                }
            }
        }

        /// <inheritdoc />
        public void Event(string eventName, Dictionary<string, string> telemetryData = null, Dictionary<string, double> metrics = null)
        {
            try
            {
                this.TelemetryClient.TrackEvent(eventName, telemetryData, metrics);
            }
            catch
            {
                if (!this.properties.SuppressAllErrors)
                {
                    throw;
                }
            }
        }

        /// <inheritdoc />
        public void Exception(Exception exception, Dictionary<string, string> telemetryData = null)
        {
            try
            {
                this.TelemetryClient.TrackException(exception);
            }
            catch
            {
                if (!this.properties.SuppressAllErrors)
                {
                    throw;
                }
            }
        }

        /// <inheritdoc />
        public void Log(LogLevel level,  string message)
        {
            try
            {
                this.TelemetryClient.TrackTrace(message, LogSeverityMapper.Map(level));
            }
            catch
            {
                if (!this.properties.SuppressAllErrors)
                {
                    throw;
                }
            }
        }
    }
}