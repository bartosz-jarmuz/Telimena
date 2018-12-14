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
        public Telimena(ITelimenaStartupInfo startupInfo)
        {
            this.startupInfo = startupInfo;
            if (this.startupInfo.TelemetryApiBaseUrl == null)
            {
                this.startupInfo.TelemetryApiBaseUrl = DefaultApiUri;
            }

            if (this.startupInfo.MainAssembly == null)
            {
                this.startupInfo.MainAssembly = GetProperCallingAssembly();
            }

            this.Async = new AsyncTelimenaHandler(this);
            this.Blocking = new BlockingTelimenaHandler(this);
            InternalStartupData data = LoadProgramData(this.startupInfo.MainAssembly, startupInfo.ProgramInfo);

            this.TelemetryKey = this.startupInfo.TelemetryKey;

            this.StaticProgramInfo = data.ProgramInfo;
            this.UserInfo = data.UserInfo;
            this.TelimenaVersion = data.TelimenaVersion;

            this.HttpClient = new TelimenaHttpClient(new HttpClient {BaseAddress = this.startupInfo.TelemetryApiBaseUrl});
            this.Messenger = new Messenger(this.Serializer, this.HttpClient);
        }
    }
}