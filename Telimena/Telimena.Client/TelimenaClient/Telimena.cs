using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using Telimena.Telemetry;

namespace TelimenaClient
{
    /// <summary>
    ///     Telemetry and Lifecycle Management Engine App
    ///     <para>This is a client SDK that allows handling application telemetry and lifecycle</para>
    /// </summary>
    public partial class Telimena : ITelimena
    {

        /// <summary>
        ///     Creates a new instance of Telimena Client
        /// </summary>
        /// <param name="startupInfo">Data object which contains startup parameters for Telimena client</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ITelimena Construct(ITelimenaStartupInfo startupInfo)
        {
            Telimena instance = new Telimena(startupInfo);
            return instance;
        }

        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        IUpdatesModule ITelimena.Updates => this.updates;

        /// <inheritdoc />
        /// ReSharper disable once ConvertToAutoProperty
        ITelemetryModule ITelimena.Telemetry => this.telemetry;

        /// <inheritdoc />
        public ITelimenaProperties Properties => this.propertiesInternal;

        /// <summary>
        ///     Creates a new instance of Telimena Client
        /// </summary>
        /// <param name="startupInfo">Data object which contains startup parameters for Telimena client</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private Telimena(ITelimenaStartupInfo startupInfo)
        {
            this.propertiesInternal= new TelimenaProperties(startupInfo);
            this.Locator = new Locator(this.Properties.StaticProgramInfo);

            this.telemetry = new TelemetryModule(this);
            this.updates = new UpdatesModule(this);

            this.httpClient = new TelimenaHttpClient(new HttpClient {BaseAddress = this.propertiesInternal.StartupInfo.TelemetryApiBaseUrl});
            this.Messenger = new Messenger(this.Serializer, this.httpClient);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        }


        
        private readonly object exceptionReportingLocker = new object();
        private static readonly List<object> UnhandledExceptionsReported = new List<object>();

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            lock (this.exceptionReportingLocker)
            {
                if (!UnhandledExceptionsReported.Contains(e.ExceptionObject))
                {
                    this.telemetry.Exception((Exception) e.ExceptionObject);
                    this.telemetry.SendAllDataNow();
                    UnhandledExceptionsReported.Add(e.ExceptionObject);
                }
            }
        }
    }
}