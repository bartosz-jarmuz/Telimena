namespace Telimena.Client
{
    #region Using
    using System;
    using System.Net.Http;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;
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
        private bool IsInitialized { get; set; }
        internal ITelimenaSerializer Serializer { get; set; } = new TelimenaSerializer();
        protected UserInfo UserInfo { get; }
        protected ProgramInfo ProgramInfo { get; }
        protected int ProgramId { get; set; }
        protected int UserId { get; set; }
        public bool SuppressAllErrors { get; set; } = true;
        protected string TelimenaVersion { get; }
        private HttpClient HttpClient { get; }

       
    }
}