using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Telimena.WebApp.Controllers
{
    using Core.Interfaces;
    using Core.Models;
    using System.Threading.Tasks;

    [Authorize(Roles = TelimenaRoles.Developer)]
    public class DeveloperDashboardController : Controller
    {
        // GET: DeveloperDashboard
        public ActionResult Index()
        {
            return View();
        }

        public Task<ActionResult> DeveloperSummary(string developerName)
        {
            return null;
        }
    }
}