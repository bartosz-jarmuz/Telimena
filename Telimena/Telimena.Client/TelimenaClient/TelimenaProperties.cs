using RandomFriendlyNameGenerator;
using System;
using System.Diagnostics;
using System.Reflection;
using TelimenaClient.Model;
using TelimenaClient.Model.Internal;

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

            this.StaticProgramInfo = info.ProgramInfo ?? new ProgramInfo { PrimaryAssembly = new Model.AssemblyInfo(this.StartupInfo.MainAssembly), Name = this.StartupInfo.MainAssembly.GetName().Name };
            this.Locator = new Locator(this.StaticProgramInfo);

            this.SuppressAllErrors = info.SuppressAllErrors;

            this.TelimenaVersion = TelimenaVersionReader.ReadToolkitVersion(Assembly.GetExecutingAssembly());

            this.InstrumentationKey = this.StartupInfo.InstrumentationKey;

        }

     




        /// <summary>
        ///     Data object containing information used at Telimena client startup
        /// </summary>
        public ITelimenaStartupInfo StartupInfo { get; }

        /// <inheritdoc />
        public UpdatePromptingModes UpdatePromptingMode { get; set; }

        /// <summary>
        ///     The unique key for this program's telemetry service
        /// </summary>
        public Guid TelemetryKey => this.StartupInfo.TelemetryKey;

        /// <summary>
        ///     Gets the user information.
        /// </summary>
        /// <value>The user information.</value>
        public UserInfo UserInfo { get; internal set; }

        private readonly Locator Locator;

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
        public Uri TelemetryApiBaseUrl => this.StartupInfo.TelemetryApiBaseUrl;

        /// <summary>
        /// Gets or sets the instrumentation key (If AppInsights is used)
        /// </summary>
        /// <value>The instrumentation key.</value>
        public string InstrumentationKey { get; internal set; }

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



    }
}