using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

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

            BuildTelemetryClient();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        }

        private readonly object telemetryClientBuildingLock = new object();

        private void BuildTelemetryClient()
        {
            lock (this.telemetryClientBuildingLock)
            {
                TelemetryConfiguration cfg = TelemetryConfiguration.Active;
                TelemetryBuffer buffer = new TelemetryBuffer();
                cfg.TelemetryChannel = new TelimenaInMemoryChannel(buffer
                    , new InMemoryTransmitter(buffer
                        , new DeliverySettings()
                        {
                            TelimenaEndpoint = new Uri(this.propertiesInternal.StartupInfo.TelemetryApiBaseUrl
                                , ApiRoutes.PostTelemetryData)
                        }));
                if (!cfg.TelemetryInitializers.Any(x => x is SequencePropertyInitializer))
                {
                    cfg.TelemetryInitializers.Add(new SequencePropertyInitializer());
                }

                var teliInitializer = cfg.TelemetryInitializers.FirstOrDefault(x => x is TelimenaPropertiesInitializer);
                if (teliInitializer != null)
                {
                    cfg.TelemetryInitializers.Remove(teliInitializer);
                }
                cfg.TelemetryInitializers.Add(new TelimenaPropertiesInitializer(this.Properties));

                cfg.InstrumentationKey = "1a14064b-d326-4ce3-939e-8cba4d08c255";
                this.telemetryClient = new TelemetryClient(cfg);
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
                    this.telemetryClient.TrackException((Exception) e.ExceptionObject);
                    this.telemetryClient.Flush();
                    UnhandledExceptionsReported.Add(e.ExceptionObject);
                }
            }
        }
    }
}