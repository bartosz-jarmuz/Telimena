using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Telimena.WebApp.Core.Models;

namespace Telimena.WebApp.Models.Updater
{
    public class ToolkitManagementViewModel
    {
        public ICollection<UpdaterPackageInfo> UpdaterPackages { get; set; } = new List<UpdaterPackageInfo>();
        public ICollection<TelimenaToolkitData> ToolkitPackages { get; set; } = new List<TelimenaToolkitData>();
    }
}