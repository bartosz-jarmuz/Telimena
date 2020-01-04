using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using DotNetLittleHelpers;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Portal;
using Telimena.WebApp.Infrastructure.Security;
using Telimena.WebApp.Infrastructure.UnitOfWork.Implementation;
using Telimena.WebApp.Models.Account;

namespace Telimena.WebApp.Controllers
{
    #region Using

    #endregion
    //dev branch test...
    /// <summary>
    /// Class AccountController.
    /// </summary>
    /// <seealso cref="System.Web.Mvc.Controller" />
    [TelimenaAuthorize]
    public class AccountController : Controller 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="logger">The logger.</param>
        public AccountController(IAccountUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        /// <summary>
        /// The unit of work
        /// </summary>
        private readonly IAccountUnitOfWork unitOfWork;
        /// <summary>
        /// The logger
        /// </summary>


        /// <summary>
        /// Changes the password.
        /// </summary>
        /// <returns>ActionResult.</returns>
        
        [HttpGet]
        public ActionResult ChangePassword()
        {
            return this.View();
        }

        /// <summary>
        /// Changes the password.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Task&lt;ActionResult&gt;.</returns>
        
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
                    TelimenaUser user = await this.unitOfWork.UserManager.FindAsync(this.User.Identity.Name, model.OldPassword).ConfigureAwait(false);
                    if (user != null)
                    {
                        IdentityResult result =
                            await this.unitOfWork.UserManager.ChangePasswordAsync(this.User.Identity.GetUserId(), model.OldPassword, model.NewPassword).ConfigureAwait(false);
                        if (result.Succeeded)
                        {
                            user.MustChangePassword = false;
                            await this.unitOfWork.UserManager.UpdateAsync(user).ConfigureAwait(false);
                            model.IsSuccess = true;
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

        /// <summary>
        /// Forgots the password.
        /// </summary>
        /// <returns>ActionResult.</returns>
        
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return this.Content("<label class=\"warning\">Contact the administrator</label>");
        }

        /// <summary>
        /// Logins this instance.
        /// </summary>
        /// <returns>ActionResult.</returns>
        [HttpGet]
        
        [AllowAnonymous]
        public ActionResult Login()
        {
            return this.View();
        }

        /// <summary>
        /// Logins the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns>Task&lt;ActionResult&gt;.</returns>
        [HttpPost]
        [AllowAnonymous]
        
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (this.ModelState.IsValid)
            {

                TelimenaUser user = await this.unitOfWork.UserManager.FindAsync(model.Email, model.Password).ConfigureAwait(false);
                if (user != null)
                {
                    if (user.IsActivated)
                    {
                        user.LastLoginDate = DateTime.UtcNow;
                        await this.unitOfWork.UserManager.UpdateAsync(user).ConfigureAwait(false);
                        ClaimsIdentity ident = await this.unitOfWork.UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie).ConfigureAwait(false);
                        this.unitOfWork.AuthManager.SignIn(new AuthenticationProperties {IsPersistent = false}, ident);
                        if (user.MustChangePassword)
                        {
                            return this.RedirectToAction("ChangePassword");
                        }

                        return this.Redirect(returnUrl ?? this.Url.Action("Index", "Home"));
                    }

                    return this.View("WaitForActivationInfo");
                }
            }

            this.ModelState.AddModelError("", "Invalid username or password");
            return this.View(model);
        }

        /// <summary>
        /// Logs the off.
        /// </summary>
        /// <returns>ActionResult.</returns>
        public ActionResult LogOff()
        {
            string username = this.User.Identity.Name;
            this.unitOfWork.AuthManager.SignOut();
            return this.RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Registers this instance.
        /// </summary>
        /// <returns>ActionResult.</returns>
        
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Register()
        {
            return this.View(new RegisterViewModel());
        }

        /// <summary>
        /// Registers the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Task&lt;ActionResult&gt;.</returns>
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

                Tuple<IdentityResult, IdentityResult> results = await this.unitOfWork.RegisterUserAsync(user, model.Password, TelimenaRoles.Viewer, model.Role).ConfigureAwait(false);
                await this.unitOfWork.CompleteAsync().ConfigureAwait(false);

                if (results.Item1.Succeeded)
                {
                    if (results.Item2 != null && results.Item2.Succeeded || results.Item2 == null)
                    {
                        return this.View("WaitForActivationInfo");
                    }

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

        /// <summary>
        /// Validates the registration model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
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