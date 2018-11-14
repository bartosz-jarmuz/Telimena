using System;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace TelimenaClient
{
    #region Using

    #endregion

    /// <summary>
    ///     Telemetry and Lifecycle Management Engine App
    ///     <para>This is a client SDK that allows handling application telemetry and lifecycle</para>
    /// </summary>
    public partial class Telimena : ITelimena
    {
        /// <summary>
        ///     Creates a new instance of Telimena Client
        /// </summary>
        /// <param name="telemetryApiBaseUrl">Leave default, unless you want to call different telemetry server</param>
        /// <param name="telemetryKey"></param>
        /// <param name="mainAssembly">
        ///     Leave null, unless you want to use different assembly as the main one for program name,
        ///     version etc
        /// </param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public Telimena(Guid telemetryKey, Assembly mainAssembly = null, Uri telemetryApiBaseUrl = null)
        {
            if (telemetryApiBaseUrl == null)
            {
                telemetryApiBaseUrl = DefaultApiUri;
            }

            if (mainAssembly == null)
            {
                mainAssembly = GetProperCallingAssembly();
            }

            StartupData data = LoadProgramData(telemetryKey, mainAssembly);
            if (data.ProgramInfo.TelemetryKey == Guid.Empty)
            {
                throw new InvalidOperationException($"Empty Guid provided as {nameof(ProgramInfo.TelemetryKey)}");
            }
            this.StaticProgramInfo = data.ProgramInfo;
            this.UserInfo = data.UserInfo;
            this.TelimenaVersion = data.TelimenaVersion;

            this.HttpClient = new TelimenaHttpClient(new HttpClient {BaseAddress = telemetryApiBaseUrl});
            this.Messenger = new Messenger(this.Serializer, this.HttpClient);
        }

        /// <summary>
        ///     Creates a new instance of Telimena Client
        /// </summary>
        /// <param name="telemetryApiBaseUrl">Leave default, unless you want to call different telemetry server</param>
        /// <param name="programInfo">
        ///   Send custom program info if you don't want assembly based approach (and if you know what you're doing) 
        /// </param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public Telimena(ProgramInfo programInfo, Uri telemetryApiBaseUrl = null)
        {
            if (telemetryApiBaseUrl == null)
            {
                telemetryApiBaseUrl = DefaultApiUri;
            }

            Assembly assembly = GetProperCallingAssembly();

            StartupData data = LoadProgramData(programInfo.TelemetryKey, assembly, programInfo);
            if (data.ProgramInfo.TelemetryKey == Guid.Empty)
            {
                throw new InvalidOperationException($"Empty Guid provided as {nameof(ProgramInfo.TelemetryKey)}");
            }
            this.StaticProgramInfo = data.ProgramInfo;
            this.UserInfo = data.UserInfo;
            this.TelimenaVersion = data.TelimenaVersion;

            this.HttpClient = new TelimenaHttpClient(new HttpClient {BaseAddress = telemetryApiBaseUrl});
            this.Messenger = new Messenger(this.Serializer, this.HttpClient);
        }
    }
} 