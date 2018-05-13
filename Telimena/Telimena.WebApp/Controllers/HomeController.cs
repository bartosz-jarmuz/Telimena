using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Telimena.WebApi.Controllers
{
    using System.Threading.Tasks;
    using WebApp.Core.Interfaces;
    using WebApp.Core.Models;
    using WebApp.Infrastructure.Security;

    [TelimenaAuthorize]
    public class HomeController : Controller
    {
        private readonly ITelimenaRepository repository;

        public HomeController(ITelimenaRepository repository)
        {
            this.repository = repository;
        }

        public ActionResult Index()
        {
            if (this.User.IsInRole(TelimenaRoles.Admin))
            {
                return this.RedirectToAction("Index", "AdminDashboard");
            }
            else
            {
                return this.RedirectToAction("Index", "DeveloperDashboard");
            }
        }

       
    }
}