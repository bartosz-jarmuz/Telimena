using System;
using System.Collections.Generic;
using System.Reflection;

namespace Telimena.Client
{
    #region Using

    #endregion

    /// <summary>
    ///     Telemetry and Lifecycle Management Engine App
    ///     <para>This is a client SDK that allows handling application telemetry and lifecycle</para>
    /// </summary>
    public partial class Telimena : ITelimena
    {
#if DEBUG
        private static readonly Uri defaultApiUri = new Uri("http://localhost:7757/");
#else
        private static readonly Uri defaultApiUri = new Uri("https://telimena-test.azurewebsites.net/");
#endif
        public UserInfo UserInfo { get; internal set; }
        public ProgramInfo ProgramInfo { get; internal set; }

        public bool SuppressAllErrors { get; set; } = true;
        public string ProgramVersion => this.ProgramInfo?.PrimaryAssembly?.Version;
        protected int ProgramId { get; set; }
        protected int UserId { get; set; }
        protected string TelimenaVersion { get; }
        internal ITelimenaSerializer Serializer { get; set; } = new TelimenaSerializer();
        internal IMessenger Messenger { get; set; }
        internal ITelimenaHttpClient HttpClient { get; set; }

        private bool IsInitialized { get; set; }

        private List<Assembly> HelperAssemblies { get; } = new List<Assembly>();
        protected string UpdaterVersion { get; set; }
    }
}