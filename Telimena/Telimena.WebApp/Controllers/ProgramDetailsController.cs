namespace Telimena.WebApi.Controllers
{
    using System;
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
        private IProgramsUnitOfWork Work { get; }

        public ProgramDetailsController(IProgramsUnitOfWork work)
        {
            this.Work = work;
        }


        [HttpGet]
        public async Task<ActionResult> Index(string programName)
        {
            if (string.IsNullOrEmpty(programName))
            {
                return this.RedirectToAction("Index", "Home");
            }
            Program program = await this.Work.Programs.FirstOrDefaultAsync(x=>x.Name == programName);

            ProgramDetailsViewModel model = new ProgramDetailsViewModel();
            if (program != null)
            {
                model.ProgramId = program.Id;
                model.ProgramName = program.Name;
            }

            model.ProgramDownloadUrl = this.Request.Url.GetLeftPart(UriPartial.Authority) + this.@Url.HttpRouteUrl("DownloadAppRoute", new {  name = model.ProgramName });

            List<UpdatePackageInfo> packages = await this.Work.UpdatePackages.GetAllPackages(model.ProgramId);
            model.UpdatePackages = packages;

            model.ProgramPackageInfo = await this.Work.ProgramPackages.GetLatestProgramPackageInfo(model.ProgramId);

            return this.View("Index", model);
        }


    }

    
}