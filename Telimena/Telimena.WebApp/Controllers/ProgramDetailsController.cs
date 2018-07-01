namespace Telimena.WebApi.Controllers
{
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using WebApp.Core.Interfaces;
    using WebApp.Core.Models;
    using WebApp.Infrastructure.Security;
    using WebApp.Infrastructure.UnitOfWork;
    using WebApp.Models.ProgramDetails;

    [TelimenaAuthorize(Roles = TelimenaRoles.Developer)]

    public class ProgramDetailsController : Controller
    {
        private readonly IProgramsUnitOfWork _work;

        public ProgramDetailsController(IProgramsUnitOfWork work)
        {
            this._work = work;
        }


        [HttpGet]
        public async Task<ActionResult> Index(string programName)
        {
            await Task.Delay(1);
            Program program = await this._work.Programs.FirstOrDefaultAsync(x=>x.Name == programName);

            var model = new ProgramDetailsViewModel()
            {
                ProgramId = program.ProgramId,
                ProgramName = program.Name
            };
            return this.View("Index", model);
        }


    }

    
}