using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MvcAuditLogger;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Portal;
using Telimena.WebApp.Infrastructure.Security;
using Telimena.WebApp.Infrastructure.UnitOfWork;
using Telimena.WebApp.Models.Shared;

namespace Telimena.WebApp.Controllers
{
    /// <summary>
    /// Class HomeController.
    /// </summary>
    /// <seealso cref="System.Web.Mvc.Controller" />
    [TelimenaAuthorize]
    public class HomeController : Controller
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="work">The work.</param>
        public HomeController(IProgramsUnitOfWork work)
        {
            this.work = work;
        }

        /// <summary>
        /// The work
        /// </summary>
        private readonly IProgramsUnitOfWork work;

        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <returns>ActionResult.</returns>
        [Audit]
        public ActionResult Index()
        {
            if (this.User.IsInRole(TelimenaRoles.Admin))
            {
                return this.RedirectToAction("Portal", "AdminDashboard");
            }

            return this.RedirectToAction("Index", "DeveloperDashboard");
        }

        /// <summary>
        /// Programses the list.
        /// </summary>
        /// <returns>PartialViewResult.</returns>
        [ChildActionOnly]
        public PartialViewResult ProgramsList()
        {
            TelimenaUser user =  this.work.Users.GetByPrincipal(this.User);
            var programs = this.work.Programs.GetProgramsVisibleToUser(user, this.User);

            ProgramsListViewModel model = new ProgramsListViewModel();

            var list = new List<ProgramMenuEntry>();
            foreach (Program program in programs)
            {
                var entry = new ProgramMenuEntry()
                {
                    ProgramName = program.Name
                    , TelemetryKey = program.TelemetryKey
                    , DeveloperTeamId = program.DeveloperTeam.Id
                    , DeveloperTeamName = program.DeveloperTeam.Name
                    ,
                };
                list.Add(entry);
            }

             model.Programs = list.GroupBy(x=>x.DeveloperTeamId);

            return this.PartialView("_ProgramsList", model);
        }

        /// <summary>
        /// Programses the list.
        /// </summary>
        /// <returns>PartialViewResult.</returns>
        [ChildActionOnly]
        public PartialViewResult TeamsList()
        {
            TelimenaUser user = this.work.Users.GetByPrincipal(this.User);
            var teams = user.AssociatedDeveloperAccounts;
            var model = new TeamsListViewModel();

            model.Teams = teams.Select(x => new TeamsMenuEntry() {TeamId = x.Id, TeamName = x.Name});
            return this.PartialView("_TeamsList", model);
        }
    }
}