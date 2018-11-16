using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using MvcAuditLogger;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Security;
using Telimena.WebApp.Infrastructure.UnitOfWork;
using Telimena.WebApp.Models.ProgramDetails;

namespace Telimena.WebApp.Controllers
{
    [TelimenaAuthorize(Roles = TelimenaRoles.Developer)]
    [RoutePrefix("ProgramManagement")]
    public class ProgramManagementController : Controller
    {
        public ProgramManagementController(IProgramsUnitOfWork work)
        {
            this.Work = work;
        }

        private IProgramsUnitOfWork Work { get; }

        [Audit]
        [HttpGet]
        [Route("{developerName}/{programName}")]
        public async Task<ActionResult> Index(string developerName, string programName)
        {
            Program program = await this.Work.Programs.SingleOrDefaultAsync(x => x.DeveloperAccount.Name == developerName && x.Name == programName);

            if (program == null)
            {
                return this.RedirectToAction("Index", "Home");
            }

            ProgramManagementViewModel model = new ProgramManagementViewModel
            {
                ProgramId = program.Id,
                TelemetryKey = program.TelemetryKey,
                ProgramName = program.Name,
                ProgramDescription = program.Description,
            };


            model.ProgramDownloadUrl = this.Request.Url.GetLeftPart(UriPartial.Authority) +
                                       this.Url.HttpRouteUrl("DownloadAppRoute", new {name = model.ProgramName});

            List<ProgramUpdatePackageInfo> packages = await this.Work.UpdatePackages.GetAllPackages(model.ProgramId);
            model.UpdatePackages = packages;

            model.ProgramPackageInfo = await this.Work.ProgramPackages.GetLatestProgramPackageInfo(model.ProgramId);

            var publicUpdaters = await this.Work.UpdaterRepository.GetPublicUpdaters();
            model.UpdatersSelectList = new List<SelectListItem>();
            foreach (Updater publicUpdater in publicUpdaters)
            {
                var item = new SelectListItem() {
                    Text = publicUpdater.InternalName,
                    Value = publicUpdater.Id.ToString()
                };
                if (publicUpdater.Id == program.Updater.Id)
                {
                    item.Selected = true;
                }
                model.UpdatersSelectList.Add(item);
               
            }

            return this.View("Index", model);
        }
    }
}