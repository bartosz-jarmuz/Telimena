using System;
using System.Diagnostics;
using Hangfire;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using MvcAuditLogger;
using Owin;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Identity;

namespace Telimena.WebApp
{
    /// <summary>
    /// Class IdentityConfig.
    /// </summary>
    public class IdentityConfig
    {
        /// <summary>
        /// Configurations the specified application.
        /// </summary>
        /// <param name="app">The application.</param>
        public void Configuration(IAppBuilder app)
        {
            app.CreatePerOwinContext(() => new TelimenaPortalContext());
            app.CreatePerOwinContext<TelimenaUserManager>(TelimenaUserManager.Create);
            LoadHangfire(app);
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    OnApplyRedirect = ctx =>
                    {
                        if (!ctx.Request.Path.StartsWithSegments(new PathString("/api")))
                        {
                            ctx.Response.Redirect(ctx.RedirectUri);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Loads the hangfire.
        /// </summary>
        /// <param name="app">The application.</param>
        private static void LoadHangfire(IAppBuilder app)
        {
            try
            {
                GlobalConfiguration.Configuration.UseSqlServerStorage("AuditingDBContext");
                app.UseHangfireDashboard();
                app.UseHangfireServer();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
    }
}