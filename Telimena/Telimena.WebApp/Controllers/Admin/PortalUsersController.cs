using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using log4net;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Identity;
using Telimena.WebApp.Models.PortalUsers;

namespace Telimena.WebApp.Controllers.Admin
{
    #region Using

    #endregion

    [Authorize(Roles = TelimenaRoles.Admin)]
    public class PortalUsersController : Controller
    {
        public PortalUsersController(ILog logger, ITelimenaUserManager userManager)
        {
            this.logger = logger;
            this.userManager = userManager;
        }

        private readonly ILog logger;
        private readonly ITelimenaUserManager userManager;

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            PortalUsersViewModel model = await this.InitializeModel();
            return this.View(model);
        }

        [HttpPost]
        public async Task<ActionResult> ToggleRoleActivation(string userId, bool activateRole, string roleName)
        {
            TelimenaUser user = await this.userManager.FindByIdAsync(userId);
            try
            {
                if (activateRole)
                {
                    await this.userManager.AddToRoleAsync(user.Id, roleName);
                }
                else
                {
                    await this.userManager.RemoveFromRoleAsync(user.Id, roleName);
                }

                bool isInRole = await this.userManager.IsInRoleAsync(user.Id, roleName);
                this.logger.Error($"User [{user.UserName}] role [{roleName}] status changed. Is in role: [{isInRole}].");
                return this.Json(isInRole);
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error while toggling role activation to [{activateRole}]. Role: [{roleName}]. User [{user.UserName}]", ex);
                return null;
            }
        }

        [HttpPost]
        public async Task<ActionResult> ToggleUserActivation(string userId, bool isActive)
        {
            TelimenaUser user = await this.userManager.FindByIdAsync(userId);
            user.IsActivated = !isActive;
            await this.userManager.UpdateAsync(user);
            this.logger.Info($"User [{user.UserName}] activation status changed to [{user.IsActivated}]");
            return this.Json(user.IsActivated);
        }

        private async Task<PortalUsersViewModel> InitializeModel()
        {
            PortalUsersViewModel model = new PortalUsersViewModel();
            List<TelimenaUser> users = await this.userManager.Users.ToListAsync();
            foreach (TelimenaUser telimenaUser in users)
            {
                TelimenaUserViewModel userViewModel = Mapper.Map<TelimenaUserViewModel>(telimenaUser);
                userViewModel.RoleNames = await this.userManager.GetRolesAsync(telimenaUser.Id);
                userViewModel.DeveloperAccountsLed = telimenaUser.GetDeveloperAccountsLedByUser().Select(x => x.Name);
                model.Users.Add(userViewModel);
            }

            return model;
        }
    }
}