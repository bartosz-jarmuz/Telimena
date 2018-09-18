using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using DotNetLittleHelpers;
using log4net;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Security;
using Telimena.WebApp.Infrastructure.UnitOfWork.Implementation;
using Telimena.WebApp.Models.Account;

namespace Telimena.WebApp.Controllers
{
    #region Using

    #endregion

    [TelimenaAuthorize]
    public class AccountController : Controller
    {
        public AccountController(IAccountUnitOfWork unitOfWork, ILog logger)
        {
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        private readonly IAccountUnitOfWork unitOfWork;
        private readonly ILog logger;

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
                    TelimenaUser user = await this.unitOfWork.UserManager.FindAsync(this.User.Identity.Name, model.OldPassword);
                    if (user != null)
                    {
                        IdentityResult result =
                            await this.unitOfWork.UserManager.ChangePasswordAsync(this.User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
                        if (result.Succeeded)
                        {
                            user.MustChangePassword = false;
                            await this.unitOfWork.UserManager.UpdateAsync(user);
                            model.IsSuccess = true;
                            this.logger.Info($"[{this.User.Identity.Name}] password changed");
                            return this.View(model);
                        }

                        foreach (string error in result.Errors)
                        {
                            this.ModelState.AddModelError("", error);
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
            return this.Content("<label class=\"warning\">Contact the administrator</label>");
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

                TelimenaUser user = await this.unitOfWork.UserManager.FindAsync(model.Email, model.Password);
                if (user != null)
                {
                    if (user.IsActivated)
                    {
                        this.logger.Info($"[{model.Email}] logged in.");
                        user.LastLoginDate = DateTime.UtcNow;
                        await this.unitOfWork.UserManager.UpdateAsync(user);
                        ClaimsIdentity ident = await this.unitOfWork.UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
                        this.unitOfWork.AuthManager.SignIn(new AuthenticationProperties {IsPersistent = false}, ident);
                        if (user.MustChangePassword)
                        {
                            return this.RedirectToAction("ChangePassword");
                        }

                        return this.Redirect(returnUrl ?? this.Url.Action("Index", "Home"));
                    }

                    this.logger.Info($"[{model.Email}] logged in but not activated");
                    return this.View("WaitForActivationInfo");
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
            this.unitOfWork.AuthManager.SignOut();
            this.logger.Info($"[{username}] logged out");
            return this.RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Register()
        {
            return this.View(new RegisterViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                if (!this.ValidateRegistrationModel(model))
                {
                    return this.View(model);
                }

                TelimenaUser user = new TelimenaUser(model.Email, model.Name);

                Tuple<IdentityResult, IdentityResult> results = await this.unitOfWork.RegisterUserAsync(user, model.Password, TelimenaRoles.Viewer, model.Role);
                await this.unitOfWork.CompleteAsync();

                if (results.Item1.Succeeded)
                {
                    if (results.Item2 != null && results.Item2.Succeeded || results.Item2 == null)
                    {
                        this.logger.Info($"User [{user.GetNameAndIdString()}] user registered and added to proper roles");
                        return this.View("WaitForActivationInfo");
                    }

                    this.logger.Info(
                        $"User [{user.GetNameAndIdString()}] user registered but errors occurred while adding to roles: [{string.Join(",", results.Item2.Errors)}");
                    foreach (string resultError in results.Item2.Errors)
                    {
                        this.ModelState.AddModelError("", resultError);
                    }
                }
                else
                {
                    foreach (string resultError in results.Item1.Errors)
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

        private bool ValidateRegistrationModel(RegisterViewModel model)
        {
            bool valid = true;
            if (!model.Email.IsValidEmail())
            {
                this.ModelState.AddModelError("", "The provided email seems invalid");
                valid = false;
            }

            if (model.ConfirmPassword != model.Password)
            {
                this.ModelState.AddModelError("", "Provided new password does not match the repeated password");
                valid = false;
            }

            if (model.Role != TelimenaRoles.Developer && model.Role != TelimenaRoles.Viewer)
            {
                this.ModelState.AddModelError("", "Incorrect role specified");
                valid = false;
            }


            return valid;
        }
    }
}