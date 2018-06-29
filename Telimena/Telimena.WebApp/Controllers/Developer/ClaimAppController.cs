using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Telimena.WebApp.Controllers.Developer
{
    using System.Threading.Tasks;
    using Core.Interfaces;
    using Core.Models;
    using Infrastructure.Repository;
    using Infrastructure.Security;
    using log4net;
    using Models.Developer;
    [TelimenaAuthorize(Roles = TelimenaRoles.Developer)]
    public class ClaimAppController : Controller
    {
        private IProgramRepository repository;
        private ILog logger;
        public ClaimAppController(IProgramRepository repository, ILog logger)
        {
            this.repository = repository;
            this.logger = logger;
        }

        [HttpGet]
        public ActionResult Claim()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Claim(ClaimAppViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model?.AppName))
            {
                this.ModelState.AddModelError("", "App Name field is required");
                return this.View(model);
            }

            var app = await this.repository.FirstOrDefaultAsync(x => x.Name == model.AppName);
            if (app == null)
            {
                this.ModelState.AddModelError("", $"App [{model.AppName}] does not exist");
                return this.View(model);
            }

           
            return this.View(model);

        }
    }
}