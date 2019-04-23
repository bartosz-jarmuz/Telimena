using System.Collections.Generic;
using Telimena.Portal.Api.Models;
using Telimena.Portal.Api.Models.DTO;

namespace Telimena.WebApp.Models.Updater
{
#pragma warning disable 1591
    public class ToolkitManagementViewModel
    {
        public ICollection<UpdaterPackageInfoDto> UpdaterPackages { get; set; } = new List<UpdaterPackageInfoDto>();
        public ICollection<TelimenaToolkitDataDto> ToolkitPackages { get; set; } = new List<TelimenaToolkitDataDto>();
    }
#pragma warning restore 1591

}