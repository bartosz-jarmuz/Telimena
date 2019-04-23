using System;

namespace Telimena.WebApp.Core.DTO.MappableToClient
{
    /// <summary>
    /// Class UpdateRequest.
    /// </summary>
    public class UpdateRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateRequest"/> class.
        /// </summary>
        [Obsolete("Only for serialization")]
        public UpdateRequest()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateRequest"/> class.
        /// </summary>
        /// <param name="telemetryKey">The program identifier.</param>
        /// <param name="programVersion">The program version.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="acceptBeta">if set to <c>true</c> [accept beta].</param>
        /// <param name="toolkitVersion">The toolkit version.</param>
        /// <param name="updaterVersion">The updater version.</param>
        public UpdateRequest(Guid telemetryKey, VersionData programVersion, Guid userId, bool acceptBeta, string toolkitVersion, string updaterVersion)
        {
            this.TelemetryKey = telemetryKey;
            this.UserId = userId;
            this.VersionData = programVersion;
            this.ToolkitVersion = toolkitVersion;
            this.UpdaterVersion = updaterVersion;
            this.AcceptBeta = acceptBeta;
        }

        /// <summary>
        /// Gets or sets the program identifier.
        /// </summary>
        /// <value>The program identifier.</value>
        public Guid TelemetryKey { get; set; }

        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>The user identifier.</value>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the program version.
        /// </summary>
        /// <value>The program version.</value>
        public VersionData VersionData { get; set; }

        /// <summary>
        /// Gets or sets the toolkit version.
        /// </summary>
        /// <value>The toolkit version.</value>
        public string ToolkitVersion { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [accept beta].
        /// </summary>
        /// <value><c>true</c> if [accept beta]; otherwise, <c>false</c>.</value>
        public bool AcceptBeta { get; set; }

        /// <summary>
        /// Gets or sets the updater version.
        /// </summary>
        /// <value>The updater version.</value>
        public string UpdaterVersion { get; set; }
    }
}