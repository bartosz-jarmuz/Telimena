using System.Net.Http;
using System.Runtime.CompilerServices;

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
        public ITelimenaProperties Properties => this.properties;

        /// <summary>
        ///     Creates a new instance of Telimena Client
        /// </summary>
        /// <param name="startupInfo">Data object which contains startup parameters for Telimena client</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private Telimena(ITelimenaStartupInfo startupInfo)
        {
            this.properties= new TelimenaProperties(startupInfo);
            this.telemetry = new TelemetryModule(this);
            this.updates= new UpdatesModule(this);
    
            this.httpClient = new TelimenaHttpClient(new HttpClient {BaseAddress = this.properties.StartupInfo.TelemetryApiBaseUrl});
            this.Messenger = new Messenger(this.Serializer, this.httpClient);
        }
    }
}