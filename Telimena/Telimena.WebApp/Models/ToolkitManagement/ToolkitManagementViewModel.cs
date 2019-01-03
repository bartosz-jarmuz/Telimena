using System.Collections.Generic;
using Telimena.WebApp.Core.Models;

namespace Telimena.WebApp.Models.Updater
{
#pragma warning disable 1591
    public class ToolkitManagementViewModel
    {
        public ICollection<UpdaterPackageInfo> UpdaterPackages { get; set; } = new List<UpdaterPackageInfo>();
        public ICollection<TelimenaToolkitData> ToolkitPackages { get; set; } = new List<TelimenaToolkitData>();
    }
#pragma warning restore 1591

}