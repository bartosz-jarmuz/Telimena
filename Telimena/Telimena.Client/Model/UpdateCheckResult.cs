using System;
using System.Collections.Generic;
using System.Linq;

namespace Telimena.Client
{
    public class UpdateCheckResult
    {
        public bool IsUpdateAvailable => this.UpdatesToInstall != null && this.UpdatesToInstall.Any();
        public Exception Error { get; set; }

        public IReadOnlyList<UpdatePackageData> UpdatesToInstall { get; set; }
    }
}