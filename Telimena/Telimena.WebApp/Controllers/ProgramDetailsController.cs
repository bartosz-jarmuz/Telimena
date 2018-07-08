namespace Telimena.WebApi.Controllers
{
    using System.Collections.Generic;
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
            if (string.IsNullOrEmpty(programName))
            {
                return this.RedirectToAction("Index", "Home");
            }
            Program program = await this._work.Programs.FirstOrDefaultAsync(x=>x.Name == programName);

            ProgramDetailsViewModel model = new ProgramDetailsViewModel();
            if (program != null)
            {
                model.ProgramId = program.ProgramId;
                model.ProgramName = program.Name;
            }

            List<UpdatePackage> packages = await this._work.UpdatePackages.GetAllPackages(model.ProgramId);
            model.UpdatePackages = packages;

            return this.View("Index", model);
        }


    }

    
}