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
        private Queue<ITelemetry> telemetryToSendLater = new Queue<ITelemetry>();

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
                if (!isSessionContextInitialized)
                {
                    this.telemetryToSendLater.Enqueue(tele);
                }
                else
                {
                    this.TelemetryClient.TrackPageView(tele);
                }
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
                EventTelemetry telemetry = new EventTelemetry(eventName);

                if (properties != null && properties.Count > 0)
                {
                    Utils.CopyDictionary(properties, telemetry.Properties);
                }

                if (metrics != null && metrics.Count > 0)
                {
                    Utils.CopyDictionary(metrics, telemetry.Metrics);
                }
                if (!isSessionContextInitialized)
                {
                    this.telemetryToSendLater.Enqueue(telemetry);
                }
                else
                {
                    this.TelemetryClient.TrackEvent(telemetry);
                }

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

                if (exception == null)
                {
                    exception = new Exception(Utils.PopulateRequiredStringValue(null, "message", typeof(ExceptionTelemetry).FullName));
                }

                var telemetry = new ExceptionTelemetry(exception);

                if (properties != null && properties.Count > 0)
                {
                    Utils.CopyDictionary(properties, telemetry.Properties);
                }

                if (metrics != null && metrics.Count > 0)
                {
                    Utils.CopyDictionary(metrics, telemetry.Metrics);
                }

                if (!isSessionContextInitialized)
                {
                    this.telemetryToSendLater.Enqueue(telemetry);
                }
                else
                {
                    this.TelemetryClient.TrackException(telemetry);
                }
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
                var telemetry = new TraceTelemetry(message, LogSeverityMapper.Map(level));

                if (!isSessionContextInitialized)
                {
                    this.telemetryToSendLater.Enqueue(telemetry);
                }
                else
                {
                    this.TelemetryClient.TrackTrace(telemetry);
                }
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