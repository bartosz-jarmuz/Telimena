using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Repository;
using Telimena.WebApp.Infrastructure.Security;

namespace Telimena.WebApp.Controllers
{
    public class ProgramsDashboardBase
    {
        public ProgramsDashboardBase(IProgramsDashboardUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        private readonly IProgramsDashboardUnitOfWork unitOfWork;

        public async Task<List<ProgramSummary>> GetAllPrograms(IPrincipal user)
        {
            TelimenaUser telimenaUser = this.unitOfWork.Users.FirstOrDefault(x => x.UserName == user.Identity.Name);
            List<Program> programs = await this.unitOfWork.Programs.GetProgramsVisibleToUserAsync(telimenaUser, user);
            List<ProgramSummary> summary = (await this.unitOfWork.GetProgramsSummary(programs)).ToList();
            return summary;
        }

        public async Task<AllProgramsSummaryData> GetAllProgramsSummaryCounts(IPrincipal user)
        {
            TelimenaUser telimenaUser = this.unitOfWork.Users.FirstOrDefault(x => x.UserName == user.Identity.Name);
            List<Program> programs = await this.unitOfWork.Programs.GetProgramsVisibleToUserAsync(telimenaUser, user);

            AllProgramsSummaryData summary = await this.unitOfWork.GetAllProgramsSummaryCounts(programs);
            return summary;
        }
    }

    [TelimenaAuthorize(Roles = TelimenaRoles.Developer)]
    public class DeveloperDashboardController : Controller
    {
        public DeveloperDashboardController(IProgramsDashboardUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
            this.dashboardBase = new ProgramsDashboardBase(unitOfWork);
        }

        private readonly IProgramsDashboardUnitOfWork unitOfWork;
        private readonly ProgramsDashboardBase dashboardBase;

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

        public ActionResult Index()
        {
            return this.View();
        }
    }



}