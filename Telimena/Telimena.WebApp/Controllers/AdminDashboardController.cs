using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Telimena.WebApp.Controllers
{
    using System.Data.Entity;
    using System.Threading.Tasks;
    using Core.Interfaces;
    using Core.Models;
    using DataTables.AspNet.Core;
    using DataTables.AspNet.Mvc5;
    using Infrastructure.Repository;
    using Infrastructure.Security;
    using log4net;

    [TelimenaAuthorize(Roles = TelimenaRoles.Admin)]
    public class AdminDashboardController : Controller
    {
        private readonly ILog logger;
        private readonly IAdminDashboardUnitOfWork unitOfWork;

        public AdminDashboardController(ILog logger, IAdminDashboardUnitOfWork unitOfWork)
        {
            this.logger = logger;
            this.unitOfWork = unitOfWork;
        }

        public async Task<ActionResult> GetPortalSummary()
        {
            var summary = await this.unitOfWork.GetPortalSummary();
            return this.PartialView("_PortalSummaryBoxes", summary);
        }

        public async Task<ActionResult> GetAllProgramsSummaryCounts()
        {
            var summary = await this.unitOfWork.GetAllProgramsSummaryCounts();
            return this.PartialView("_AllProgramsSummaryBoxes", summary);
        }

        [HttpPost]
        public async Task<ActionResult> GetAllPrograms(IDataTablesRequest request)
        {
            IEnumerable<Program> summary = await this.unitOfWork.Programs.GetAllAsync();

            var response = DataTablesResponse.Create(request, summary.Count(), summary.Count(), summary);

            return new DataTablesJsonResult(response);
        }

        public ActionResult Portal()
        {
            return this.View();
        }

        public ActionResult Apps()
        {
            return this.View();
        }
    }
}