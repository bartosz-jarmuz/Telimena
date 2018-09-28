using System;
using System.Collections.Generic;
using System.Reflection;
using TelimenaClient.Serializer;

namespace TelimenaClient
{
    #region Using

    #endregion

    /// <summary>
    /// Telemetry and Lifecycle Management Engine App
    /// <para>This is a client SDK that allows handling application telemetry and lifecycle</para>
    /// </summary>
    public partial class Telimena : ITelimena
    {
#if DEBUG
        /// <summary>
        /// The default API URI
        /// </summary>
        private static readonly Uri DefaultApiUri = new Uri("http://localhost:7757/");
#else
        private static readonly Uri DefaultApiUri = new Uri("https://telimena-test.azurewebsites.net/");
#endif
        /// <summary>
        /// Gets the user information.
        /// </summary>
        /// <value>The user information.</value>
        public UserInfo UserInfo { get; internal set; }
        /// <summary>
        /// Gets the program information.
        /// </summary>
        /// <value>The program information.</value>
        public ProgramInfo ProgramInfo { get; internal set; }
        /// <summary>
        /// If true, then Telimena will swallow any errors. Otherwise, it will rethrow
        /// </summary>
        /// <value><c>true</c> if [suppress all errors]; otherwise, <c>false</c>.</value>
        public bool SuppressAllErrors { get; set; } = true;
        /// <summary>
        /// Gets the program version.
        /// </summary>
        /// <value>The program version.</value>
        public string ProgramVersion => this.ProgramInfo?.PrimaryAssembly?.Version;
        /// <summary>
        /// Gets or sets the program identifier.
        /// </summary>
        /// <value>The program identifier.</value>
        protected int ProgramId { get; set; }
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>The user identifier.</value>
        protected int UserId { get; set; }
        /// <summary>
        /// Gets the telimena version.
        /// </summary>
        /// <value>The telimena version.</value>
        protected string TelimenaVersion { get; }
        /// <summary>
        /// Gets or sets the serializer.
        /// </summary>
        /// <value>The serializer.</value>
        internal ITelimenaSerializer Serializer { get; set; } = new TelimenaSerializer();
        /// <summary>
        /// Gets or sets the messenger.
        /// </summary>
        /// <value>The messenger.</value>
        internal IMessenger Messenger { get; set; }
        /// <summary>
        /// Gets or sets the HTTP client.
        /// </summary>
        /// <value>The HTTP client.</value>
        internal ITelimenaHttpClient HttpClient { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is initialized.
        /// </summary>
        /// <value><c>true</c> if this instance is initialized; otherwise, <c>false</c>.</value>
        private bool IsInitialized { get; set; }

        /// <summary>
        /// Gets the helper assemblies.
        /// </summary>
        /// <value>The helper assemblies.</value>
        private List<Assembly> HelperAssemblies { get; } = new List<Assembly>();
        /// <summary>
        /// Gets or sets the updater version.
        /// </summary>
        /// <value>The updater version.</value>
        protected string UpdaterVersion { get; set; }
    }
}