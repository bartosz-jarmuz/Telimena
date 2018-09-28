using System;
using System.Collections.Generic;
using System.Linq;

namespace Telimena.ToolkitClient
{
    /// <summary>
    /// Class UpdateCheckResult.
    /// </summary>
    public class UpdateCheckResult
    {
        /// <summary>
        /// Gets a value indicating whether this instance is update available.
        /// </summary>
        /// <value><c>true</c> if this instance is update available; otherwise, <c>false</c>.</value>
        public bool IsUpdateAvailable => this.ProgramUpdatesToInstall != null && this.ProgramUpdatesToInstall.Any();
        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        /// <value>The exception.</value>
        public Exception Exception { get; set; }

        /// <summary>
        /// Gets or sets the program updates to install.
        /// </summary>
        /// <value>The program updates to install.</value>
        public IReadOnlyList<UpdatePackageData> ProgramUpdatesToInstall { get; set; }
        /// <summary>
        /// Gets or sets the updater update.
        /// </summary>
        /// <value>The updater update.</value>
        public UpdatePackageData UpdaterUpdate { get; set; }
    }
}