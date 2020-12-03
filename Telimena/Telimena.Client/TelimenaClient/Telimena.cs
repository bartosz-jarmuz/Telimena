using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using TelimenaClient.Model;

namespace TelimenaClient
{
    /// <summary>
    ///     Tracking and Lifecycle Management Engine App
    ///     <para>This is a client SDK that allows handling application telemetryModule and lifecycle</para>
    /// </summary>
    public partial class Telimena : ITelimena
    {  
        /// <summary>
        ///     Creates a new instance of Telimena Client. Equivalent to TelimenaFactory.Construct();
        /// </summary>
        /// <param name="startupInfo">Data object which contains startup parameters for Telimena client</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ITelimena Construct(ITelimenaStartupInfo startupInfo)
        {
            return TelimenaFactory.Construct(startupInfo);
        }
        
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
                this.SetConnectionSecurityProtocol();

                this.propertiesInternal = new TelimenaProperties(startupInfo);
                this.Locator = new Locator(this.Properties.StaticProgramInfo);
                this.userTrackingController = new UserTrackingController( this.propertiesInternal,this.Locator , this.Serializer);

                this.httpClient = new TelimenaHttpClient(new HttpClient
                {
                    BaseAddress = this.propertiesInternal.StartupInfo.TelemetryApiBaseUrl
                });
                this.Messenger = new Messenger(this.Serializer, this.httpClient);

                this.telemetryModule = new TelemetryModule(this.propertiesInternal);

                this.updates = new UpdatesModule(this);

                this.telemetryModule.InitializeTelemetryClient();
                if (startupInfo.RegisterUnhandledExceptionsTracking)
                {
                    AppDomain.CurrentDomain.UnhandledException += this.CurrentDomain_UnhandledException;
                }
            }
            catch (Exception e)
            {
                TelemetryDebugWriter.WriteError($"Error while initializing {nameof(Telimena)}. Error: {e}");
                //above all, we don't want to throw errors in client apps.
                //No telemetry is better than boom.
                throw;
            }
        }

        private void SetConnectionSecurityProtocol()
        {
            //without this, Azure App Service tends to throw a socket connection 'an existing connection was forcibly closed by the remote host'
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        }

        private readonly object exceptionReportingLocker = new object();
        private static readonly List<object> UnhandledExceptionsReported = new List<object>();

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            lock (this.exceptionReportingLocker)
            {
                if (!UnhandledExceptionsReported.Contains(e.ExceptionObject))
                {
                    this.telemetryModule.Exception((Exception) e.ExceptionObject, null,new Dictionary<string, string>()
                    {
                        { DefaultToolkitNames.ExceptionUnhandledByUserCodeKey, true.ToString() }
                    });
                    this.telemetryModule.SendAllDataNow();
                    UnhandledExceptionsReported.Add(e.ExceptionObject);
                }
            }
        }
    }
}