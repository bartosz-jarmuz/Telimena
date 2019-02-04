using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
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
        /// <param name="telemetryApiBaseUrl">The base url for the telemetry API</param>
        public TelimenaStartupInfo(Guid telemetryKey, Uri telemetryApiBaseUrl = null)
        {
            this.TelemetryKey = telemetryKey;
            this.TelemetryApiBaseUrl = telemetryApiBaseUrl;
            if (this.TelemetryApiBaseUrl == null)
            {
                this.TelemetryApiBaseUrl = Telimena.DefaultApiUri;
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
        /// Loads the helper assembly infos
        /// </summary>
        /// <param name="assemblies"></param>
        public void LoadHelperAssemblies(params Assembly[] assemblies)
        {
            this.LoadAssemblyInfos(assemblies);
        }

        /// <summary>
        ///     Gets the helper assemblies infos.
        /// </summary>
        /// <value>The helper assemblies.</value>
        public List<Model.AssemblyInfo> HelperAssemblies { get; private set; } = new List<Model.AssemblyInfo>();

        /// <summary>
        /// Gets or sets the instrumentation key (if AppInsights is in use)
        /// </summary>
        /// <value>The instrumentation key.</value>
        public string InstrumentationKey { get; set; }

        /// <inheritdoc />
        public bool RegisterUnhandledExceptionsTracking { get; set; } = true;

        /// <summary>
        /// Loads the helper assemblies based on Assembly Name
        /// </summary>
        /// <param name="assemblyNames"></param>
        public void LoadHelperAssembliesByName(params string[] assemblyNames)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            var assemblies = new List<Assembly>();
            foreach (string assemblyName in assemblyNames)
            {
                assemblies.Add(Assembly.LoadFrom(Path.Combine(path, assemblyName)));
            }
            this.LoadAssemblyInfos(assemblies);
        }

        private void LoadAssemblyInfos(IEnumerable<Assembly> assemblies)
        {
            this.HelperAssemblies = new List<Model.AssemblyInfo>();
            foreach (Assembly assembly in assemblies)
            {
                this.HelperAssemblies.Add(new Model.AssemblyInfo(assembly));
            }
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