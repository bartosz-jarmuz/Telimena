using System.Collections.Generic;
using System.Linq;
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
        public AdminDashboardController(ILog logger, IAdminDashboardUnitOfWork unitOfWork)
        {
            this.logger = logger;
            this.unitOfWork = unitOfWork;
        }

        private readonly ILog logger;
        private readonly IAdminDashboardUnitOfWork unitOfWork;

        public ActionResult Apps()
        {
            return this.View();
        }

        [HttpPost]
        public async Task<ActionResult> GetAllPrograms()
        {
            List<ProgramSummary> summary = (await this.unitOfWork.GetProgramsSummary()).ToList();
            return this.Content(JsonConvert.SerializeObject(summary));
        }

        public async Task<ActionResult> GetAllProgramsSummaryCounts()
        {
            AllProgramsSummaryData summary = await this.unitOfWork.GetAllProgramsSummaryCounts();
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