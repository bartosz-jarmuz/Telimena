using System;
using System.Collections.Generic;
using TelimenaClient.Model;

namespace TelimenaClient
{
    /// <summary>
    /// Telemetry module that does not do anything - in case telemetry is disabled
    /// </summary>
    public class NoActionTelemetryModule : ITelemetryModule
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="properties"></param>
        /// <param name="metrics"></param>
        public void Event(string eventName, Dictionary<string, string> properties = null, Dictionary<string, double> metrics = null)
        {
            //this is a non-action object, in case telemetry is disabled
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="note"></param>
        /// <param name="properties"></param>
        /// <param name="metrics"></param>
        public void Exception(Exception exception, string note = null, Dictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            //this is a non-action object, in case telemetry is disabled
        }


        /// <summary>
        /// 
        /// </summary>
        public void InitializeTelemetryClient()
        {
            //this is a non-action object, in case telemetry is disabled

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message"></param>
        public void Log(LogLevel level, string message)
        { //this is a non-action object, in case telemetry is disabled
        }


        /// <summary>
        /// 
        /// </summary>
        public void SendAllDataNow()
        {
            //this is a non-action object, in case telemetry is disabled
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewName"></param>
        /// <param name="properties"></param>
        /// <param name="metrics"></param>
        public void View(string viewName, Dictionary<string, string> properties = null, Dictionary<string, double> metrics = null)
        {
            //this is a non-action object, in case telemetry is disabled
        }
    }
}