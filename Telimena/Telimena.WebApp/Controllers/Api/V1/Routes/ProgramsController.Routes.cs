using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace Telimena.WebApp.Controllers.Api.V1
{
    #region Using

    #endregion

    public partial class ProgramsController
    {
#pragma warning disable 1591
        public static class Routes
        {
            public const string UpdateCheck = nameof(ProgramsController) + "." + nameof(ProgramsController.UpdateCheck);
            public const string GetLatestVersionInfo = nameof(ProgramsController) + "." + nameof(ProgramsController.GetLatestVersionInfo);
            public const string SetUpdater = nameof(ProgramsController) + "." + nameof(ProgramsController.SetUpdater);
            public const string GetProgramUpdaterName = nameof(ProgramsController) + "." + nameof(ProgramsController.GetProgramUpdaterName);
            public const string GetVersionsCount = nameof(ProgramsController) + "." + nameof(ProgramsController.GetVersionsCount);
            public const string Delete = nameof(ProgramsController) + "." + nameof(ProgramsController.Delete);
            public const string Upload = nameof(ProgramsController) + "." + nameof(ProgramsController.Upload);
            public const string DownloadLatestProgramPackage = nameof(ProgramsController) + "." + nameof(ProgramsController.DownloadLatestProgramPackage);
            public const string DownloadApp = nameof(ProgramsController) + "." + nameof(ProgramsController.DownloadApp);
        }
#pragma warning restore 1591

    }
}