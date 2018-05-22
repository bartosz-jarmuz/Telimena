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
        private bool IsInitialized { get; set; }
        internal ITelimenaSerializer Serializer { get; set; } = new TelimenaSerializer();
        internal IMessenger Messenger { get; set; }
        protected UserInfo UserInfo { get; }
        protected ProgramInfo ProgramInfo { get; }
        protected int ProgramId { get; set; }
        protected int UserId { get; set; }
        public bool SuppressAllErrors { get; set; } = true;
        protected string TelimenaVersion { get; }
        private HttpClient HttpClient { get; }

       
    }
}