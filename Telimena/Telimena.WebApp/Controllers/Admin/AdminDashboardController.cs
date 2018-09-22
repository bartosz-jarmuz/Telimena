using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using log4net;
using Newtonsoft.Json;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Infrastructure.Repository;
using Telimena.WebApp.Infrastructure.Security;

namespace Telimena.WebApp.Controllers
{
    [TelimenaAuthorize(Roles = TelimenaRoles.Admin)]
    public class AdminDashboardController : Controller
    {
        public AdminDashboardController(ILog logger, IProgramsDashboardUnitOfWork unitOfWork)
        {
            this.logger = logger;
            this.unitOfWork = unitOfWork;
            this.dashboardBase = new ProgramsDashboardBase(unitOfWork);
        }

        private readonly ILog logger;
        private readonly IProgramsDashboardUnitOfWork unitOfWork;
        private readonly ProgramsDashboardBase dashboardBase;

        public ActionResult Apps()
        {
            return this.View();
        }

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

        public async Task<ActionResult> GetPortalSummary()
        {
            PortalSummaryData summary = await this.unitOfWork.GetPortalSummary();
            return this.PartialView("_PortalSummaryBoxes", summary);
        }

        public ActionResult Portal()
        {
            return this.View();
        }
    }
}