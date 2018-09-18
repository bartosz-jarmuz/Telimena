using System.Collections.Generic;
using System.Web.Mvc;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Security;
using Telimena.WebApp.Infrastructure.UnitOfWork;
using Telimena.WebApp.Models.Shared;

namespace Telimena.WebApi.Controllers
{
    [TelimenaAuthorize]
    public class HomeController : Controller
    {
        public HomeController(IProgramsUnitOfWork work)
        {
            this._work = work;
        }

        private readonly IProgramsUnitOfWork _work;

        public ActionResult Index()
        {
            if (this.User.IsInRole(TelimenaRoles.Admin))
            {
                return this.RedirectToAction("Portal", "AdminDashboard");
            }

            return this.RedirectToAction("Index", "DeveloperDashboard");
        }

        [ChildActionOnly]
        public PartialViewResult ProgramsList()
        {
            TelimenaUser user = this._work.Users.FirstOrDefault(x => x.UserName == this.User.Identity.Name);
            IEnumerable<Program> programs = this._work.Programs.GetProgramsVisibleToUser(user, this.User);

            ProgramsListViewModel model = new ProgramsListViewModel();
            foreach (Program program in programs)
            {
                if (!model.Programs.ContainsKey(program.Id))
                {
                    model.Programs.Add(program.Id, program.Name);
                }
            }

            return this.PartialView("_ProgramsList", model);
        }
    }
}