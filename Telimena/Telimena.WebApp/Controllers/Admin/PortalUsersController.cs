using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using log4net;
using MvcAuditLogger;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Portal;
using Telimena.WebApp.Infrastructure.Identity;
using Telimena.WebApp.Models.PortalUsers;

namespace Telimena.WebApp.Controllers.Admin
{
    #region Using

    #endregion

    /// <summary>
    /// Class PortalUsersController.
    /// </summary>
    /// <seealso cref="System.Web.Mvc.Controller" />
    [Authorize(Roles = TelimenaRoles.Admin)]
    public class PortalUsersController : Controller
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PortalUsersController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="userManager">The user manager.</param>
        public PortalUsersController(ILog logger, ITelimenaUserManager userManager)
        {
            this.logger = logger;
            this.userManager = userManager;
        }

        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILog logger;
        /// <summary>
        /// The user manager
        /// </summary>
        private readonly ITelimenaUserManager userManager;

        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <returns>Task&lt;ActionResult&gt;.</returns>
        [HttpGet]
        [Audit]
        public async Task<ActionResult> Index()
        {
            PortalUsersViewModel model = await this.InitializeModel().ConfigureAwait(false);
            return this.View(model);
        }

        /// <summary>
        /// Toggles the role activation.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="activateRole">if set to <c>true</c> [activate role].</param>
        /// <param name="roleName">Name of the role.</param>
        /// <returns>Task&lt;ActionResult&gt;.</returns>
        [Audit]
        [HttpPost]
        public async Task<ActionResult> ToggleRoleActivation(string userId, bool activateRole, string roleName)
        {
            TelimenaUser user = await this.userManager.FindByIdAsync(userId).ConfigureAwait(false);
            try
            {
                if (activateRole)
                {
                    await this.userManager.AddToRoleAsync(user.Id, roleName).ConfigureAwait(false);
                }
                else
                {
                    await this.userManager.RemoveFromRoleAsync(user.Id, roleName).ConfigureAwait(false);
                }

                bool isInRole = await this.userManager.IsInRoleAsync(user.Id, roleName).ConfigureAwait(false);
                this.logger.Error($"User [{user.UserName}] role [{roleName}] status changed. Is in role: [{isInRole}].");
                return this.Json(isInRole);
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error while toggling role activation to [{activateRole}]. Role: [{roleName}]. User [{user.UserName}]", ex);
                return null;
            }
        }

        /// <summary>
        /// Toggles the user activation.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="isActive">if set to <c>true</c> [is active].</param>
        /// <returns>Task&lt;ActionResult&gt;.</returns>
        [Audit]
        [HttpPost]
        public async Task<ActionResult> ToggleUserActivation(string userId, bool isActive)
        {
            TelimenaUser user = await this.userManager.FindByIdAsync(userId).ConfigureAwait(false);
            user.IsActivated = !isActive;
            await this.userManager.UpdateAsync(user).ConfigureAwait(false);
            this.logger.Info($"User [{user.UserName}] activation status changed to [{user.IsActivated}]");
            return this.Json(user.IsActivated);
        }

        /// <summary>
        /// Initializes the model.
        /// </summary>
        /// <returns>Task&lt;PortalUsersViewModel&gt;.</returns>
        private async Task<PortalUsersViewModel> InitializeModel()
        {
            PortalUsersViewModel model = new PortalUsersViewModel();
            List<TelimenaUser> users = await this.userManager.Users.ToListAsync().ConfigureAwait(false);
            foreach (TelimenaUser telimenaUser in users)
            {
                TelimenaUserViewModel userViewModel = Mapper.Map<TelimenaUserViewModel>(telimenaUser);
                userViewModel.RoleNames = await this.userManager.GetRolesAsync(telimenaUser.Id).ConfigureAwait(false);
                userViewModel.DeveloperAccountsLed = telimenaUser.GetDeveloperAccountsLedByUser().Select(x => x.Name);
                model.Users.Add(userViewModel);
            }

            return model;
        }
    }
}