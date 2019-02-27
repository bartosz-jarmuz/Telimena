using System;

namespace Telimena.Portal.Api.Models.DTO
{
    public class TelimenaToolkitDataDto
    {
        public Guid Id { get; set; }

        public string Version { get; set; }

        public TelimenaPackageInfoDto TelimenaPackageInfo { get; set; }
    }
}