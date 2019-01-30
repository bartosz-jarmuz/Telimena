using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telimena.WebApp.Core.Models;

namespace Telimena.WebApp.Infrastructure
{
    public static class Router
    {
        public static class Api
        {
            private const string DownloadProgramUpdatePackage = "api/v1/update-packages";
            private const string DownloadToolkitUpdatePackage = "api/v1/toolkit";
            private const string DownloadUpdaterUpdatePackage = "api/v1/updaters";

            public static string DownloadProgramUpdate(ProgramUpdatePackageInfo pkg)
            {
                return $"{DownloadProgramUpdatePackage}/{pkg.Guid}";
            }

            public static string DownloadUpdaterUpdate(UpdaterPackageInfo pkg)
            {
                return $"{DownloadUpdaterUpdatePackage}/{pkg.Guid}";
            }

            public static string DownloadToolkitUpdate(TelimenaPackageInfo pkg)
            {
                return $"{DownloadToolkitUpdatePackage}/{pkg.Guid}";
            }
        }
    }
}
