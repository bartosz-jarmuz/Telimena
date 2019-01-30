﻿using System.Collections.Generic;
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