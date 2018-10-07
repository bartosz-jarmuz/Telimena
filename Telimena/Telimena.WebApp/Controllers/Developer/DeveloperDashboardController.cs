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
    [TelimenaAuthorize(Roles = TelimenaRoles.Developer)]
    public class DeveloperDashboardController : Controller
    {
        public DeveloperDashboardController(IProgramsDashboardUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
            this.dashboardBase = new ProgramsDashboardBase(unitOfWork);
        }

        private readonly IProgramsDashboardUnitOfWork unitOfWork;
        private readonly ProgramsDashboardBase dashboardBase;

        [HttpPost]
        public async Task<ActionResult> GetAllPrograms()
        {
            List<ProgramSummary> programs = await this.dashboardBase.GetAllPrograms(this.User);
            return this.Content(JsonConvert.SerializeObject(programs));
        }

        public async Task<ActionResult> GetAllProgramsSummaryCounts()
        {
            AllProgramsSummaryData summary = await this.dashboardBase.GetAllProgramsSummaryCounts(this.User);
            return this.PartialView("_AllProgramsSummaryBoxes", summary);
        }

        [Audit]
        public ActionResult Index()
        {
            return this.View();
        }
    }



}