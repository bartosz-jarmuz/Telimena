using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Telimena.WebApp.Controllers
{
    using System.Threading.Tasks;
    using Core.Interfaces;
    using Infrastructure.Security;
    using log4net;

    [TelimenaAuthorize(Roles = TelimenaRoles.Admin)]
    public class AdminDashboardController : Controller
    {
        private readonly ILog logger;
        private readonly ITelimenaRepository repository;

        public AdminDashboardController(ILog logger, ITelimenaRepository repository)
        {
            this.logger = logger;
            this.repository = repository;
        }

        public async Task<ActionResult> GetPortalSummary()
        {
            var summary = await this.repository.GetPortalSummary();
            return this.PartialView("_PortalSummaryBoxes", summary);
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