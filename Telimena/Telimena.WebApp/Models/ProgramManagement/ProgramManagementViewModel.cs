#pragma warning disable 1591

using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Telimena.Portal.Api.Models.DTO;

namespace Telimena.WebApp.Models.ProgramManagement
{
    public class ProgramManagementViewModel
    {
        public string ProgramName { get; set; }
        public int ProgramId { get; set; }
        public string PrimaryAssemblyName { get; set; }
        public Guid TelemetryKey { get; set; }
        public string InstrumentationKey { get; set; }

        public ICollection<ProgramUpdatePackageInfoDto> UpdatePackages { get; set; } = new List<ProgramUpdatePackageInfoDto>();  

        public string ProgramDownloadUrl { get; set; }
        public ProgramPackageInfoDto ProgramPackageInfo { get; set; }

        public List<SelectListItem> UpdatersSelectList { get; set; }
        public string ProgramDescription { get; set; }

        public Dictionary<string, string> UpdaterInfo { get; set; }= new Dictionary<string, string>();
    }
}