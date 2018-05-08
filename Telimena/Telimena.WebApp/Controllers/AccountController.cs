using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Telimena.WebApp.Controllers
{
    using System.Threading.Tasks;
    using Infrastructure.Identity;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.Owin;
    using Microsoft.Owin.Security;
    using Models.Account;

    public class AccountController : Controller
    {
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login()
        {
            return this.View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (this.ModelState.IsValid)
            {
                var userManager = this.HttpContext.GetOwinContext().GetUserManager<TelimenaUserManager>();
                var authManager = this.HttpContext.GetOwinContext().Authentication;

                TelimenaUser user = userManager.Find(model.Email, model.Password);
                if (user != null)
                {
                    if (user.IsActivated)
                    {

                        var ident = userManager.CreateIdentity(user,
                            DefaultAuthenticationTypes.ApplicationCookie);
                        authManager.SignIn(
                            new AuthenticationProperties {IsPersistent = false}, ident);
                        return this.Redirect(returnUrl ?? this.Url.Action("Index", "Home"));
                    }
                    else
                    {
                        return this.View("WaitForActivationInfo");
                    }
                }
            }

            this.ModelState.AddModelError("", "Invalid username or password");
            return this.View(model);
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
            if (ModelState.IsValid)
            {
                var user = new TelimenaUser { UserName = model.Email, Email = model.Email };
                var userManager = this.HttpContext.GetOwinContext().GetUserManager<TelimenaUserManager>();

                var result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    return View("WaitForActivationInfo");
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
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return null;
        }

    }
}