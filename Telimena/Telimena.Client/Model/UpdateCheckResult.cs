using System;
using System.Collections.Generic;
using System.Linq;

namespace Telimena.Client
{
    public class UpdateCheckResult
    {
        public bool IsUpdateAvailable => this.ProgramUpdatesToInstall != null && this.ProgramUpdatesToInstall.Any();
        public Exception Error { get; set; }

        public IReadOnlyList<UpdatePackageData> ProgramUpdatesToInstall { get; set; }
        public UpdatePackageData UpdaterUpdate { get; set; }
    }
}