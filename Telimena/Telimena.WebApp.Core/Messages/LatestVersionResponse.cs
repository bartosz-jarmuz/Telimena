using System;
using System.Collections.Generic;
using Telimena.WebApp.Core.DTO;

namespace Telimena.WebApp.Core.Messages
{
    public class LatestVersionResponse 
    {
        public VersionInfo PrimaryAssemblyVersion { get; set; }
        public ICollection<VersionInfo> HelperAssemblyVersions { get; set; }

        public Exception Error { get; set; }
    }

}