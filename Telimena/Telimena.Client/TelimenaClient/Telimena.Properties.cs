namespace Telimena.Client
{
    #region Using
    using System;
    using System.Collections.Generic;
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
        private const string DefaultApiUri = "http://localhost:7757/";

        private bool IsInitialized { get; set; }
        internal ITelimenaSerializer Serializer { get; set; } = new TelimenaSerializer();
        internal IMessenger Messenger { get; set; }
        public UserInfo UserInfo { get; internal set; }
        public ProgramInfo ProgramInfo { get; internal set; }
        protected int ProgramId { get; set; }
        protected int UserId { get; set; }
        public bool SuppressAllErrors { get; set; } = true;
        protected string TelimenaVersion { get; }
        internal ITelimenaHttpClient HttpClient { get; set; }

        private List<Assembly> HelperAssemblies { get; set; } = new List<Assembly>();
    }
}