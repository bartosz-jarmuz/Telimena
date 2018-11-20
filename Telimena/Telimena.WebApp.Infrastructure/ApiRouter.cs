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
            private const string DownloadProgramUpdatePackage = "api/ProgramUpdates/Get";
            private const string DownloadToolkitUpdatePackage = "api/Toolkit/Get";
            private const string DownloadUpdaterUpdatePackage = "api/Updater/Get";

            public static string DownloadProgramUpdate(ProgramUpdatePackageInfo pkg)
            {
                return $"{DownloadProgramUpdatePackage}?id={pkg.Guid}";
            }

            public static string DownloadUpdaterUpdate(UpdaterPackageInfo pkg)
            {
                return $"{DownloadUpdaterUpdatePackage}?id={pkg.Guid}";
            }

            public static string DownloadToolkitUpdate(TelimenaPackageInfo pkg)
            {
                return $"{DownloadToolkitUpdatePackage}?id={pkg.Guid}";
            }
        }
    }
}
