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
        public void View(string viewName, Dictionary<string, string> properties = null, Dictionary<string, double> metrics = null)
        {
            try
            {
                PageViewTelemetry tele = new PageViewTelemetry(viewName);
                if (properties != null)
                {
                    Utils.CopyDictionary(properties, tele.Properties);
                }
                if (metrics != null)
                {
                    Utils.CopyDictionary(metrics, tele.Metrics);
                }
                this.TelemetryClient.TrackPageView(tele);
            }
            catch
            {
                if (!this.telimenaProperties.SuppressAllErrors)
                {
                    throw;
                }
            }
        }

        /// <inheritdoc />
        public void Event(string eventName, Dictionary<string, string> properties = null, Dictionary<string, double> metrics = null)
        {
            try
            {
                this.TelemetryClient.TrackEvent(eventName, properties, metrics);
            }
            catch
            {
                if (!this.telimenaProperties.SuppressAllErrors)
                {
                    throw;
                }
            }
        }

        /// <inheritdoc />
        public void Exception(Exception exception, string note = null, Dictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(note))
                {
                    if (properties == null)
                    {
                        properties = new Dictionary<string, string>();
                        properties.Add(DefaultToolkitNames.TelimenaCustomExceptionNoteKey, note);
                    }
                    else
                    {
                        properties.Add(DefaultToolkitNames.TelimenaCustomExceptionNoteKey, note);
                    }
                }
                
                this.TelemetryClient.TrackException(exception, properties, metrics);
            }
            catch
            {
                if (!this.telimenaProperties.SuppressAllErrors)
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
                if (!this.telimenaProperties.SuppressAllErrors)
                {
                    throw;
                }
            }
        }
    }
}