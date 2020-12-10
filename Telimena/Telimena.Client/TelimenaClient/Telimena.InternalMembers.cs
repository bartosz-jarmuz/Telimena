using System;
using TelimenaClient.Model;
using TelimenaClient.Serializer;

namespace TelimenaClient
{
    /// <summary>
    ///     Tracking and Lifecycle Management Engine App
    ///     <para>This is a client SDK that allows handling application telemetryModule and lifecycle</para>
    /// </summary>
    public partial class Telimena : ITelimena
    {
        /// <summary>
        ///     The default API URI
        /// </summary>
        internal static readonly Uri DefaultApiUri = new Uri("http://localhost:7757/");


        /// <summary>
        ///     Gets or sets the serializer.
        /// </summary>
        /// <value>The serializer.</value>
        internal ITelimenaSerializer Serializer { get; set; } = new TelimenaSerializer();

        /// <summary>
        ///     Gets or sets the messenger.
        /// </summary>
        /// <value>The messenger.</value>
        internal IMessenger Messenger { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is initialized.
        /// </summary>
        /// <value><c>true</c> if this instance is initialized; otherwise, <c>false</c>.</value>
        private bool IsInitialized { get; set; }

        /// <summary>
        ///     Locator provides paths to toolkit files and folders.
        /// </summary>
        /// <value>The locator.</value>
        internal Locator Locator { get; }

        private readonly ITelimenaHttpClient httpClient;
        private TelemetryInitializeResponse initializationResponse;
        internal readonly ITelemetryModule telemetryModule;
        private readonly IUpdatesModule updates;
        private readonly TelimenaProperties propertiesInternal;
        private readonly UserTrackingController userTrackingController;
    }
}