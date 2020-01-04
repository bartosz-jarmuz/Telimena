using System;
using System.Collections.Generic;
using System.IdentityModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Mvc;
using AutoMapper;
using Telimena.Portal.Api.Models.DTO;
using Telimena.WebApp.Controllers.Api.V1;
using Telimena.WebApp.Controllers.Api.V1.Helpers;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Portal;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;
using Telimena.WebApp.Infrastructure.Security;
using Telimena.WebApp.Infrastructure.UnitOfWork;
using Telimena.WebApp.Models.ProgramManagement;

namespace Telimena.WebApp.Controllers
{
    /// <summary>
    /// Class ProgramManagementController.
    /// </summary>
    /// <seealso cref="System.Web.Mvc.Controller" />
    [TelimenaAuthorize(Roles = TelimenaRoles.Developer)]
    public class ProgramManagementController : Controller
    {
        private readonly IFileRetriever fileRetriever;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramManagementController"/> class.
        /// </summary>
        /// <param name="work">The work.</param>
        public ProgramManagementController(IProgramsUnitOfWork work, IFileRetriever fileRetriever)
        {
            this.Work = work;
            this.fileRetriever = fileRetriever;

        }

        /// <summary>
        /// Gets the work.
        /// </summary>
        /// <value>The work.</value>
        private IProgramsUnitOfWork Work { get; }

        /// <summary>
        /// Indexes the specified telemetry key.
        /// </summary>
        /// <param name="telemetryKey">The telemetry key.</param>
        /// <returns>Task&lt;ActionResult&gt;.</returns>
        
        [System.Web.Mvc.HttpGet]
        public async Task<ActionResult> Index(Guid telemetryKey)
        {
            Program program = await this.Work.Programs.GetByTelemetryKey(telemetryKey).ConfigureAwait(false);

            if (program == null)
            {
                return this.RedirectToAction("Index", "Home");
            }

            ProgramManagementViewModel model = new ProgramManagementViewModel
            {
                TelemetryKey = program.TelemetryKey,
                InstrumentationKey = program.InstrumentationKey,
                ProgramName = program.Name,
                ProgramId = program.Id,
                PrimaryAssemblyName = program.PrimaryAssembly.Name + program.PrimaryAssembly.Extension,
                ProgramDescription = program.Description,
            };

            model.ProgramDownloadUrl = this.Request.Url.GetLeftPart(UriPartial.Authority) +
                                       this.Url.NeutralApiUrl(ProgramsController.Routes.DownloadApp, new {developerName = program.DeveloperTeam.Name, programName = model.ProgramName});

            List<ProgramUpdatePackageInfo> packages = await this.Work.UpdatePackages.GetAllPackages(program.Id).ConfigureAwait(false);
            model.UpdatePackages = Mapper.Map<List<ProgramUpdatePackageInfoDto>>(packages);

            model.ProgramPackageInfo = Mapper.Map<ProgramPackageInfoDto>(await this.Work.ProgramPackages.GetLatestProgramPackageInfo(program.Id).ConfigureAwait(false));

            var publicUpdaters = await this.Work.UpdaterRepository.GetPublicUpdaters().ConfigureAwait(false);
            model.UpdatersSelectList = new List<SelectListItem>();
            foreach (Updater publicUpdater in publicUpdaters)
            {
                var item = new SelectListItem() {
                    Text = publicUpdater.InternalName,
                    Value = publicUpdater.PublicId.ToString()

                };
                if (publicUpdater.PublicId == program.Updater.PublicId)
                {
                    item.Selected = true;
                }
                model.UpdatersSelectList.Add(item);

                if (!model.UpdaterInfo.ContainsKey(publicUpdater.InternalName))
                {
                    model.UpdaterInfo.Add(publicUpdater.InternalName, publicUpdater.Description);
                }
            }

            return this.View("ManageProgram", model);
        }


        
        [System.Web.Mvc.HttpGet, System.Web.Mvc.Route("~/{developerName}/{programName}/get", Name ="Get")]
        public async Task<IHttpActionResult> DownloadApp(string developerName, string programName)
        {
            Program prg = await this.Work.Programs.GetByNames(developerName, programName).ConfigureAwait(false);
            if (prg == null)
            {
                throw new BadRequestException($"Program [{programName}] does not exist");
            }
            return await ProgramsControllerHelpers.GetDownloadLatestProgramPackageResponse(this.Work, prg.Id, this.fileRetriever).ConfigureAwait(false);
        }
    }
}