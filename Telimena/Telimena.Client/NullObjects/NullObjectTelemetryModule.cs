using System;
using System.Collections.Generic;
using TelimenaClient.Model;

namespace TelimenaClient
{
    /// <summary>
    /// This is for when initialization of proper telemetry module would fail - we should never throw errors in client code!
    /// </summary>
    internal class NullObjectTelemetryModule : ITelemetryModule
    {
        public void Log(LogLevel level, string message)
        {
            // This is for when initialization of proper telemetry module would fail - we should never throw errors in client code!
        }

        public void Event(string eventName, Dictionary<string, string> properties = null, Dictionary<string, double> metrics = null)
        {
            // This is for when initialization of proper telemetry module would fail - we should never throw errors in client code!
        }

        public void View(string viewName, Dictionary<string, string> properties = null, Dictionary<string, double> metrics = null)
        {
            // This is for when initialization of proper telemetry module would fail - we should never throw errors in client code!
        }

        public void Exception(Exception exception, string note = null, Dictionary<string, string> properties = null
            , IDictionary<string, double> metrics = null)
        {
            // This is for when initialization of proper telemetry module would fail - we should never throw errors in client code!
        }

        public void SendAllDataNow()
        {
            // This is for when initialization of proper telemetry module would fail - we should never throw errors in client code!
        }
    }
}