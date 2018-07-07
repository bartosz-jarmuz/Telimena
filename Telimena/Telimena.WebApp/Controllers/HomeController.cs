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
    using WebApp.Infrastructure.UnitOfWork;
    using WebApp.Models.Shared;

    [TelimenaAuthorize]
    public class HomeController : Controller
    {
        private readonly IProgramsUnitOfWork _work;

        public HomeController(IProgramsUnitOfWork work)
        {
            this._work = work;
        }

        public ActionResult Index()
        {
            if (this.User.IsInRole(TelimenaRoles.Admin))
            {
                return this.RedirectToAction("Portal", "AdminDashboard");
            }
            else
            {
                return this.RedirectToAction("Index", "DeveloperDashboard");
            }
        }

        [ChildActionOnly]
        public PartialViewResult ProgramsList()
        {
            TelimenaUser user = this._work.Users.FirstOrDefault(x=>x.UserName == this.User.Identity.Name);
            IEnumerable<Program> programs = this._work.Programs.GetProgramsVisibleToUser(user, this.User);
            
            var model = new ProgramsListViewModel();
            foreach (Program program in programs)
            {
                if (!model.Programs.ContainsKey(program.ProgramId))
                {
                    model.Programs.Add(program.ProgramId, program.Name);
                }
            }

            return this.PartialView("_ProgramsList", model);

        }
    }
}