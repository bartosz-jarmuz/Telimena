#pragma warning disable 1591

using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Telimena.WebApp.Core.Models;

namespace Telimena.WebApp.Models.ProgramManagement
{
    public class ProgramManagementViewModel
    {
        public string ProgramName { get; set; }
        public string PrimaryAssemblyName { get; set; }
        public Guid TelemetryKey { get; set; }

        public ICollection<ProgramUpdatePackageInfo> UpdatePackages { get; set; } = new List<ProgramUpdatePackageInfo>();  

        public string ProgramDownloadUrl { get; set; }
        public ProgramPackageInfo ProgramPackageInfo { get; set; }

        public List<SelectListItem> UpdatersSelectList { get; set; }
        public string ProgramDescription { get; set; }
    }
}