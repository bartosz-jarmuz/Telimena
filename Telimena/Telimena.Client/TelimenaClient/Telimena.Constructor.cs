namespace Telimena.Client
{
    #region Using
    using System;
    using System.Net.Http;
    using System.Reflection;

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
        /// <param name="mainAssembly">
        ///     Leave null, unless you want to use different assembly as the main one for program name,
        ///     version etc
        /// </param>
        public Telimena(Assembly mainAssembly = null, string telemetryApiBaseUrl = DefaultApiUri)
        {
            Tuple<ProgramInfo, UserInfo, string> data = Telimena.LoadProgramData(mainAssembly);
            this.ProgramInfo = data.Item1;
            this.UserInfo = data.Item2;
            this.TelimenaVersion = data.Item3;

            this.HttpClient = new TelimenaHttpClient(new HttpClient()
            {
                BaseAddress = new Uri(telemetryApiBaseUrl)
            });
            this.Messenger = new Messenger(this.Serializer, this.HttpClient, this.SuppressAllErrors);
        }

        public Telimena(ProgramInfo programInfo, string telemetryApiBaseUrl = DefaultApiUri)
        {
            Tuple<ProgramInfo, UserInfo, string> data = Telimena.LoadProgramData(programInfo: programInfo);
            this.ProgramInfo = data.Item1;
            this.UserInfo = data.Item2;
            this.TelimenaVersion = data.Item3;

            this.HttpClient = new TelimenaHttpClient(new HttpClient()
            {
                BaseAddress = new Uri(telemetryApiBaseUrl)
            });
            this.Messenger = new Messenger(this.Serializer, this.HttpClient, this.SuppressAllErrors);
        }

    }
}