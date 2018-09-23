using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Security;
using Telimena.WebApp.Infrastructure.UnitOfWork;
using Telimena.WebApp.Models.ProgramDetails;

namespace Telimena.WebApp.Controllers
{
    [TelimenaAuthorize(Roles = TelimenaRoles.Developer)]
    public class ProgramManagementController : Controller
    {
        public ProgramManagementController(IProgramsUnitOfWork work)
        {
            this.Work = work;
        }

        private IProgramsUnitOfWork Work { get; }

        [HttpGet]
        public async Task<ActionResult> Index(string programName)
        {
            if (string.IsNullOrEmpty(programName))
            {
                return this.RedirectToAction("Index", "Home");
            }

            Program program = await this.Work.Programs.FirstOrDefaultAsync(x => x.Name == programName);

            ProgramManagementViewModel model = new ProgramManagementViewModel();
            if (program != null)
            {
                model.ProgramId = program.Id;
                model.ProgramName = program.Name;
            }

            model.ProgramDownloadUrl = this.Request.Url.GetLeftPart(UriPartial.Authority) +
                                       this.Url.HttpRouteUrl("DownloadAppRoute", new {name = model.ProgramName});

            List<ProgramUpdatePackageInfo> packages = await this.Work.UpdatePackages.GetAllPackages(model.ProgramId);
            model.UpdatePackages = packages;

            model.ProgramPackageInfo = await this.Work.ProgramPackages.GetLatestProgramPackageInfo(model.ProgramId);

            return this.View("Index", model);
        }
    }
}