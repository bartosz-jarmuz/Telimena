namespace Telimena.WebApp.Controllers
{
    #region Using
    using System;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Core.Interfaces;
    using Infrastructure.Identity;
    using Infrastructure.Security;
    using log4net;
    using Microsoft.AspNet.Identity;
    using Microsoft.Owin.Security;
    using Models.Account;
    #endregion

    [TelimenaAuthorize]
    public class AccountController : Controller
    {
        public AccountController(IAuthenticationManager authManager, ITelimenaUserManager userManager, ILog logger)
        {
            this.logger = logger;
            this.userManager = userManager;
            this.authManager = authManager;
        }

        private readonly IAuthenticationManager authManager;
        private readonly ILog logger;
        private readonly ITelimenaUserManager userManager;

        [HttpGet]
        public ActionResult ChangePassword()
        {
            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                if (model.NewPasswordRepeated != model.NewPassword)
                {
                    this.ModelState.AddModelError("", "Provided new password does not match the repeated password");
                }
                else if (model.OldPassword == model.NewPassword)
                {
                    this.ModelState.AddModelError("", "New password must be different from the old one");
                }
                else
                {
                    TelimenaUser user = await this.userManager.FindAsync(this.User.Identity.Name, model.OldPassword);
                    if (user != null)
                    {
                        var result = await this.userManager.ChangePasswordAsync(this.User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
                        if (result.Succeeded)
                        {
                            user.MustChangePassword = false;
                            await this.userManager.UpdateAsync(user);
                            model.IsSuccess = true;
                            this.logger.Info($"[{this.User.Identity.Name}] password changed");
                            return this.View(model);
                        }
                        else
                        {
                            foreach (var error in result.Errors)
                            {
                                this.ModelState.AddModelError("", error);
                            }
                        }
                    }
                    else
                    {
                        this.ModelState.AddModelError("", "Incorrect current password provided");
                    }
                }
            }

            return this.View(model);
        }

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return null;
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login()
        {
            return this.View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (this.ModelState.IsValid)
            {
                this.logger.Info($"[{model.Email}] login attempt");

                TelimenaUser user = await this.userManager.FindAsync(model.Email, model.Password);
                if (user != null)
                {
                    if (user.IsActivated)
                    {
                        this.logger.Info($"[{model.Email}] logged in.");
                        user.LastLoginDate = DateTime.UtcNow;
                        await this.userManager.UpdateAsync(user);
                        var ident = await this.userManager.CreateIdentityAsync(user,
                            DefaultAuthenticationTypes.ApplicationCookie);
                        this.authManager.SignIn(
                            new AuthenticationProperties {IsPersistent = false}, ident);
                        if (user.MustChangePassword)
                        {
                            return this.RedirectToAction("ChangePassword");
                        }

                        return this.Redirect(returnUrl ?? this.Url.Action("Index", "Home"));
                    }
                    else
                    {
                        this.logger.Info($"[{model.Email}] logged in but not activated");
                        return this.View("WaitForActivationInfo");
                    }
                }
            }

            this.ModelState.AddModelError("", "Invalid username or password");
            return this.View(model);
        }

        /// <summary>
        ///     Logs the off.
        /// </summary>
        /// <returns>ActionResult.</returns>
        public ActionResult LogOff()
        {
            string username = this.User.Identity.Name;
            this.authManager.SignOut();
            this.logger.Info($"[{username}] logged out");
            return this.RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Register()
        {
            return this.View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var user = new TelimenaUser {UserName = model.Email,
                    Email = model.Email,
                    DisplayName = model.Name,
                    CreatedDate = DateTime.UtcNow
                };

                var result = await this.userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    this.logger.Info($"[{model.Email}] user registered");

                    var addedUser =  await this.userManager.FindByIdAsync(user.Id);
                    var roleresult = await this.userManager.AddToRoleAsync(addedUser.Id, TelimenaRoles.Viewer);
                    if (!roleresult.Succeeded)
                    {
                        this.logger.Error($"Failed to add user [{user.UserName}] to {TelimenaRoles.Viewer} role. {string.Join(", ", roleresult.Errors)}");
                    }
                    return this.View("WaitForActivationInfo");
                }
                else
                {
                    foreach (string resultError in result.Errors)
                    {
                        this.ModelState.AddModelError("", resultError);
                    }
                }
            }
            else
            {
                this.ModelState.AddModelError("", "Something went wrong during registration");
            }

            // If we got this far, something failed, redisplay form
            return this.View(model);
        }
    }
}