namespace Telimena.WebApp.Controllers
{
    #region Using
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Core.Interfaces;
    using Infrastructure.Identity;
    using log4net;
    using Microsoft.AspNet.Identity;
    using Microsoft.Owin.Security;
    using Models.Account;
    using Models.PortalUsers;
    #endregion

    [Authorize(Roles = TelimenaRoles.Admin)]
    public class PortalUsersController : Controller
    {
        public PortalUsersController(ITelimenaRepository repository, ILog logger, ITelimenaUserManager userManager)
        {
            this.repository = repository;
            this.logger = logger;
            this.userManager = userManager;
        }

        private readonly IAuthenticationManager authManager;
        private readonly ILog logger;
        private readonly ITelimenaUserManager userManager;
        private ITelimenaRepository repository;

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            PortalUsersViewModel model = await this.InitializeModel();
            return this.View(model);
        }

        [HttpPost]
        public async Task<ActionResult> ToggleUserActivation(string userId, bool isActive)
        {
            var user = await this.userManager.FindByIdAsync(userId);
            user.IsActivated = !isActive;
            await this.userManager.UpdateAsync(user);
            this.logger.Info($"User [{user.UserName}] activation status changed to [{user.IsActivated}]");
            return this.Json(user.IsActivated);
        }

        [HttpPost]
        public async Task<ActionResult> ToggleRoleActivation(string userId, bool activateRole, string roleName)
        {
            var user = await this.userManager.FindByIdAsync(userId);
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

        private async Task<PortalUsersViewModel> InitializeModel()
        {
            var model = new PortalUsersViewModel();
            List<TelimenaUser> users = await this.userManager.Users.ToListAsync();
            foreach (TelimenaUser telimenaUser in users)
            {
                telimenaUser.RoleNames = await this.userManager.GetRolesAsync(telimenaUser.Id);
            }
            model.Users = users;
            
            return model;
        }
    }
}