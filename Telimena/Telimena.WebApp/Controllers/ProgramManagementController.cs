using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using MvcAuditLogger;
using Telimena.WebApp.Controllers.Api.V1;
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

        [Audit]
        [HttpGet]
        public async Task<ActionResult> Index(Guid telemetryKey)
        {
            Program program = await this.Work.Programs.SingleOrDefaultAsync(x => x.TelemetryKey == telemetryKey).ConfigureAwait(false);

            if (program == null)
            {
                return this.RedirectToAction("Index", "Home");
            }

            ProgramManagementViewModel model = new ProgramManagementViewModel
            {
                TelemetryKey = program.TelemetryKey,
                ProgramName = program.Name,
                PrimaryAssemblyName = program.PrimaryAssembly.Name + program.PrimaryAssembly.Extension,
                ProgramDescription = program.Description,
            };

            model.ProgramDownloadUrl = this.Request.Url.GetLeftPart(UriPartial.Authority) +
                                       this.Url.LatestApiUrl(ProgramsController.Routes.DownloadApp, new {developerName = program.DeveloperAccount.Name, programName = model.ProgramName});

            List<ProgramUpdatePackageInfo> packages = await this.Work.UpdatePackages.GetAllPackages(program.Id).ConfigureAwait(false);
            model.UpdatePackages = packages;

            model.ProgramPackageInfo = await this.Work.ProgramPackages.GetLatestProgramPackageInfo(program.Id).ConfigureAwait(false);

            var publicUpdaters = await this.Work.UpdaterRepository.GetPublicUpdaters().ConfigureAwait(false);
            model.UpdatersSelectList = new List<SelectListItem>();
            foreach (Updater publicUpdater in publicUpdaters)
            {
                var item = new SelectListItem() {
                    Text = publicUpdater.InternalName,
                    Value = publicUpdater.Guid.ToString()
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