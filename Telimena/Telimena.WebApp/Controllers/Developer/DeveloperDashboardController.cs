using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using MvcAuditLogger;
using Newtonsoft.Json;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Infrastructure.Repository;
using Telimena.WebApp.Infrastructure.Security;

namespace Telimena.WebApp.Controllers.Developer
{
    /// <summary>
    /// Class DeveloperDashboardController.
    /// </summary>
    /// <seealso cref="System.Web.Mvc.Controller" />
    [TelimenaAuthorize(Roles = TelimenaRoles.Developer)]
    public class DeveloperDashboardController : Controller
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeveloperDashboardController" /> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        public DeveloperDashboardController(IProgramsDashboardUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
            this.dashboardBase = new ProgramsDashboardBase(unitOfWork);
        }

        /// <summary>
        /// The unit of work
        /// </summary>
        private readonly IProgramsDashboardUnitOfWork unitOfWork;
        /// <summary>
        /// The dashboard base
        /// </summary>
        private readonly ProgramsDashboardBase dashboardBase;

        /// <summary>
        /// Gets all programs.
        /// </summary>
        /// <returns>Task&lt;ActionResult&gt;.</returns>
        [HttpPost]
        public async Task<ActionResult> GetAllProgramsUsages()
        {
            List<ProgramUsageSummary> programs = await this.dashboardBase.GetAllProgramsUsages(this.User).ConfigureAwait(false);
            return this.Content(JsonConvert.SerializeObject(programs));
        }

        /// <summary>
        /// Gets all programs summary.
        /// </summary>
        /// <returns>Task&lt;ActionResult&gt;.</returns>
        [HttpPost]
        public async Task<ActionResult> GetAllProgramsSummary()
        {
            List<ProgramSummary> programs = await this.dashboardBase.GetAllProgramsSummary(this.User).ConfigureAwait(false);
            return this.Content(JsonConvert.SerializeObject(programs));
        }


        /// <summary>
        /// Gets the application users summary.
        /// </summary>
        /// <returns>Task&lt;ActionResult&gt;.</returns>
        [HttpPost]
        public async Task<ActionResult> GetAppUsersSummary()
        {
            List<AppUsersSummaryData> programs = await this.dashboardBase.GetAppUsersSummary(this.User).ConfigureAwait(false);
            return this.Content(JsonConvert.SerializeObject(programs));
        }


        /// <summary>
        /// Gets all programs summary counts.
        /// </summary>
        /// <returns>Task&lt;ActionResult&gt;.</returns>
        public async Task<ActionResult> GetAllProgramsSummaryCounts()
        {
            AllProgramsSummaryData summary = await this.dashboardBase.GetAllProgramsSummaryCounts(this.User).ConfigureAwait(false);
            return this.PartialView("_AllProgramsSummaryBoxes", summary);
        }

        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <returns>ActionResult.</returns>
        [Audit]
        public ActionResult Index()
        {
            return this.View();
        }
    }



}