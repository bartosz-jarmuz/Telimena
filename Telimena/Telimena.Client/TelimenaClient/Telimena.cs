using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;

namespace TelimenaClient
{
    /// <summary>
    ///     Tracking and Lifecycle Management Engine App
    ///     <para>This is a client SDK that allows handling application telemetryModule and lifecycle</para>
    /// </summary>
    public partial class Telimena : ITelimena
    {
        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        IUpdatesModule ITelimena.Update => this.updates;

        /// <inheritdoc />
        /// ReSharper disable once ConvertToAutoProperty
        ITelemetryModule ITelimena.Track => this.telemetryModule;

        /// <inheritdoc />
        public ITelimenaProperties Properties => this.propertiesInternal;

        /// <summary>
        ///     Creates a new instance of Telimena Client
        /// </summary>
        /// <param name="startupInfo">Data object which contains startup parameters for Telimena client</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal Telimena(ITelimenaStartupInfo startupInfo)
        {
            try
            {
                this.propertiesInternal = new TelimenaProperties(startupInfo);
                this.Locator = new Locator(this.Properties.StaticProgramInfo);

                this.telemetryModule = new TelemetryModule(this.Properties);
                this.updates = new UpdatesModule(this);

                this.httpClient = new TelimenaHttpClient(new HttpClient
                {
                    BaseAddress = this.propertiesInternal.StartupInfo.TelemetryApiBaseUrl
                });
                this.Messenger = new Messenger(this.Serializer, this.httpClient);

                ((TelemetryModule) this.telemetryModule).InitializeTelemetryClient();
                if (startupInfo.RegisterUnhandledExceptionsTracking)
                {
                    AppDomain.CurrentDomain.UnhandledException += this.CurrentDomain_UnhandledException;
                }
            }
            catch
            {
                //above all, we don't want to throw errors in client apps.
                //No telemetry is better than boom.
            }
        }

        private readonly object exceptionReportingLocker = new object();
        private static readonly List<object> UnhandledExceptionsReported = new List<object>();

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            lock (this.exceptionReportingLocker)
            {
                if (!UnhandledExceptionsReported.Contains(e.ExceptionObject))
                {
                    this.telemetryModule.Exception((Exception) e.ExceptionObject);
                    this.telemetryModule.SendAllDataNow();
                    UnhandledExceptionsReported.Add(e.ExceptionObject);
                }
            }
        }
    }
}