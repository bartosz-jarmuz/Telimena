using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Security;
using Telimena.WebApp.Infrastructure.UnitOfWork;
using Telimena.WebApp.Models.Updater;
using TelimenaClient;

namespace Telimena.WebApp.Controllers.Admin
{
    [TelimenaAuthorize(Roles = TelimenaRoles.Admin)]
    public class ToolkitManagementController : Controller
    {
        public ToolkitManagementController(IToolkitDataUnitOfWork work)
        {
            this.work = work;
        }

        private readonly IToolkitDataUnitOfWork work;

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            IEnumerable<UpdaterPackageInfo> updaterPackageInfos = await this.work.UpdaterRepository.GetPackages(DefaultToolkitNames.UpdaterInternalName);
            IEnumerable<TelimenaToolkitData> toolkitData =
                await this.work.ToolkitDataRepository.GetAsync(includeProperties: nameof(TelimenaToolkitData.TelimenaPackageInfo));
            ToolkitManagementViewModel model = new ToolkitManagementViewModel
            {
                UpdaterPackages = updaterPackageInfos.ToList(), ToolkitPackages = toolkitData.ToList()
            };
            return this.View(model);
        }
    }
}