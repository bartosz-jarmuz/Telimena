using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Security;
using Telimena.WebApp.Infrastructure.UnitOfWork;
using Telimena.WebApp.Models.Updater;

namespace Telimena.WebApp.Controllers.Admin
{
    /// <summary>
    /// Class ToolkitManagementController.
    /// </summary>
    /// <seealso cref="System.Web.Mvc.Controller" />
    [TelimenaAuthorize(Roles = TelimenaRoles.Admin)]
    public class ToolkitManagementController : Controller
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToolkitManagementController"/> class.
        /// </summary>
        /// <param name="work">The work.</param>
        public ToolkitManagementController(IToolkitDataUnitOfWork work)
        {
            this.work = work;
        }

        /// <summary>
        /// The work
        /// </summary>
        private readonly IToolkitDataUnitOfWork work;

        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <returns>Task&lt;ActionResult&gt;.</returns>
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            IEnumerable<UpdaterPackageInfo> updaterPackageInfos = await this.work.UpdaterRepository.GetAllPackages().ConfigureAwait(false);
            IEnumerable<TelimenaToolkitData> toolkitData =
                await this.work.ToolkitDataRepository.GetAsync(includeProperties: nameof(TelimenaToolkitData.TelimenaPackageInfo)).ConfigureAwait(false);
            ToolkitManagementViewModel model = new ToolkitManagementViewModel
            {
                UpdaterPackages = updaterPackageInfos.ToList(), ToolkitPackages = toolkitData.ToList()
            };
            return this.View(model);
        }
    }
}