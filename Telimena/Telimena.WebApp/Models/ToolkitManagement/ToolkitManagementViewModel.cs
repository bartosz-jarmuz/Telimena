using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Telimena.WebApp.Core.Models;

namespace Telimena.WebApp.Models.Updater
{
    public class ToolkitManagementViewModel
    {
        public ICollection<UpdaterInfo> UpdaterPackages { get; set; } = new List<UpdaterInfo>();
    }
}