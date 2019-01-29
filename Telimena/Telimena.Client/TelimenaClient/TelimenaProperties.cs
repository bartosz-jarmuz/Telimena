using System;
using System.Diagnostics;
using System.Reflection;

namespace TelimenaClient
{
    /// <summary>
    ///     Properties of Telimena
    /// </summary>
    public class TelimenaProperties : ITelimenaProperties
    {

        /// <summary>
        ///     Creates new instance
        /// </summary>
        /// <param name="info"></param>
        public TelimenaProperties(ITelimenaStartupInfo info)
        {
            this.StartupInfo = info;
            this.TelemetryKey = this.StartupInfo.TelemetryKey;
            this.InstrumentationKey = this.StartupInfo.InstrumentationKey;

            if (this.StartupInfo.TelemetryApiBaseUrl == null)
            {
                this.StartupInfo.TelemetryApiBaseUrl = Telimena.DefaultApiUri;
            }
            this.TelemetryApiBaseUrl = this.StartupInfo.TelemetryApiBaseUrl;

            if (this.StartupInfo.MainAssembly == null)
            {
                this.StartupInfo.MainAssembly = GetProperCallingAssembly();
            }
            
            this.StaticProgramInfo = info.ProgramInfo ?? new ProgramInfo { PrimaryAssembly = new AssemblyInfo(this.StartupInfo.MainAssembly), Name = this.StartupInfo.MainAssembly.GetName().Name };
            this.UserInfo = info.UserInfo ?? new UserInfo { UserName = Environment.UserName, MachineName = Environment.MachineName };

            this.StaticProgramInfo.HelperAssemblies = info.HelperAssemblies;
            this.SuppressAllErrors = info.SuppressAllErrors;

            this.TelimenaVersion = TelimenaVersionReader.ReadToolkitVersion(Assembly.GetExecutingAssembly());
        }

        /// <summary>
        ///     Data object containing information used at Telimena client startup
        /// </summary>
        public ITelimenaStartupInfo StartupInfo { get; }

        /// <summary>
        ///     The unique key for this program's telemetry service
        /// </summary>
        public Guid TelemetryKey { get; private set; }

        /// <summary>
        ///     Gets the user information.
        /// </summary>
        /// <value>The user information.</value>
        public UserInfo UserInfo { get; internal set; }

        /// <summary>
        ///     Gets the program information from the assemblies.
        /// </summary>
        /// <value>The program information.</value>
        public ProgramInfo StaticProgramInfo { get; internal set; }

        /// <summary>
        ///     Gets the live program information
        /// </summary>
        /// <value>The live program information.</value>
        public LiveProgramInfo LiveProgramInfo { get; internal set; }

        /// <summary>
        /// Gets the telimena base URL.
        /// </summary>
        /// <value>The telimena base URL.</value>
        public Uri TelemetryApiBaseUrl { get; internal set; }

        /// <summary>
        /// Gets or sets the instrumentation key (If AppInsights is used)
        /// </summary>
        /// <value>The instrumentation key.</value>
        public string InstrumentationKey { get; set; }

        /// <summary>
        ///     Gets the program version.
        /// </summary>
        /// <value>The program version.</value>
        public VersionData ProgramVersion => this.StaticProgramInfo?.PrimaryAssembly?.VersionData;

        /// <summary>
        ///     Gets the telimena version.
        /// </summary>
        /// <value>The telimena version.</value>
        public string TelimenaVersion { get; set; }

        /// <summary>
        /// Default: TRUE <para/>
        /// If set to true Telimena will not throw any unhandled exceptions. Otherwise, errors will be thrown (for debug purpose)
        /// </summary>
        public bool SuppressAllErrors { get; set; } = true;

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