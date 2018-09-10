using System.Linq;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;
using Telimena.WebApp.Core.Messages;
using Telimena.WebApp.Infrastructure.Repository;
using Telimena.WebApp.Models.Updater;

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

    [TelimenaAuthorize(Roles = TelimenaRoles.Admin)]

    public class ToolkitManagementController : Controller
    {
        private readonly ITelimenaToolkitDataUnitOfWork work;

        public ToolkitManagementController(ITelimenaToolkitDataUnitOfWork work)
        {
            this.work = work;
        }


        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var infos = await this.work.UpdaterRepository.GetAsync();
            var model = new ToolkitManagementViewModel();
            model.UpdaterPackages = infos.ToList();
            return this.View(model);
        }

      
    }

    
}