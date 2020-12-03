using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using TelimenaClient.Model;

namespace TelimenaClient
{
    /// <summary>
    /// A simple data object containing the data needed for Telimena startup, and some initial settings
    /// </summary>
    public class TelimenaStartupInfo : ITelimenaStartupInfo
    {
        /// <summary>
        /// Creates default instance of the simple data object containing the data needed for Telimena startup, and some initial settings
        /// </summary>
        /// <param name="telemetryKey">The telemetry key for the current app</param>
        /// <param name="telemetryApiBaseUrl">The base url for the telemetry API. You can either:<br/>
        ///    - hardcode it here <br/>
        ///    - add an app.config key 'TelimenaUrl' with the URL as value<br/>
        ///    - or add a file called 'TelimenaUrl' with the URL as content (in the executable location)</param>
        /// 
        public TelimenaStartupInfo(Guid telemetryKey, Uri telemetryApiBaseUrl = null)
        {
            this.TelemetryKey = telemetryKey;

            //the configurable value should have precedence over any hardcoded values
            Uri configuredUri = GetTelemetryUriFromConfig(telemetryApiBaseUrl);
            if (configuredUri != null)
            {
                this.TelemetryApiBaseUrl = configuredUri;
            }
            if (this.TelemetryApiBaseUrl == null)
            {
                this.TelemetryApiBaseUrl = telemetryApiBaseUrl;
            }
            if (this.MainAssembly == null)
            {
                this.MainAssembly = GetProperCallingAssembly();
            }
        }


        /// <summary>
        /// Gets or sets the interval between batches of telemetry being sent. Default value is 30 seconds.
        /// </summary>
        /// <value>The delivery interval.</value>
        public TimeSpan DeliveryInterval { get; set; } = TimeSpan.FromSeconds(30);

        /// <inheritdoc />
        public Guid TelemetryKey { get; set; }

        /// <inheritdoc />
        public Uri TelemetryApiBaseUrl { get; set; }

        /// <inheritdoc />
        public Assembly MainAssembly { get; set; }

        /// <inheritdoc />
        public ProgramInfo ProgramInfo { get; set; }

        /// <inheritdoc />
        public UserInfo UserInfo { get; set; }

        /// <inheritdoc />
        public bool SuppressAllErrors { get; set; } = true;

        /// <summary>
        /// Gets or sets the instrumentation key (if AppInsights is in use)
        /// </summary>
        /// <value>The instrumentation key.</value>
        public string InstrumentationKey { get; set; }

        /// <inheritdoc />
        public bool RegisterUnhandledExceptionsTracking { get; set; } = true;

        private static string TelimenaUrlSettingsKey = "TelimenaUrl";

        private static Uri GetTelemetryUriFromConfig(Uri telemetryApiBaseUrl)
        {
            var setting = ConfigurationManager.AppSettings.Get(TelimenaUrlSettingsKey);
            if (!string.IsNullOrEmpty(setting))
            {
                try
                {
                    var uri = new Uri(setting);
                    return uri;
                }
                catch (Exception ex)
                {
                    TelemetryDebugWriter.WriteLine($"ERROR - Cannot convert AppSetting [{setting}] to URI. Telimena will NOT WORK. Error: {ex}");
                }
            }
            var path = Path.Combine(Directory.GetCurrentDirectory(), TelimenaUrlSettingsKey);
            if (File.Exists(path))
            {
                var text = File.ReadAllText(path);
                try
                {
                    var uri = new Uri(text);
                    return uri;
                }
                catch (Exception ex)
                {
                    TelemetryDebugWriter.WriteError($"ERROR - Cannot convert content of file {path} - [{text}] to URI. Telimena will NOT WORK. Error: {ex}");
                    return null;
                }    
            }

            if (telemetryApiBaseUrl == null)
            {
                string message = $"ERROR - Telimena URL not specified. " +
                             $"Either add AppSetting [{TelimenaUrlSettingsKey}] or create a [{TelimenaUrlSettingsKey}] file in your app working directory." +
                             $"The setting value/file content should be JUST THE BASE URL to Telimena instance.";
                TelemetryDebugWriter.WriteError(message);
            }
            

            return null;

        }

        private static Assembly GetProperCallingAssembly()
        {
            StackTrace stackTrace = new StackTrace();
            int index = 1;
            AssemblyName currentAss = typeof(Telimena).Assembly.GetName();
            while (true)
            {
                MethodBase method = stackTrace.GetFrame(index)?.GetMethod();
                if (method?.DeclaringType?.Assembly.GetName().Name != currentAss.Name)
                {
                    string name = method?.DeclaringType?.Assembly?.GetName()?.Name;
                    if (name != null && name != "mscorlib")
                    {
                        return method.DeclaringType.Assembly;
                    }
                }

                index++;
            }
        }
    }
}