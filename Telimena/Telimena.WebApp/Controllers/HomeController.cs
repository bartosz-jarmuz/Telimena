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

    [Authorize]
    public class HomeController : Controller
    {
        private readonly ITelimenaRepository repository;

        public HomeController(ITelimenaRepository repository)
        {
            this.repository = repository;
        }

        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> DeveloperSummary(string developerName)
        {
            IEnumerable<Program> programs = await this.repository.GetDeveloperPrograms(developerName);
            return null;
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}