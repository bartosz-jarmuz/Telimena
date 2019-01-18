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
    public class IdentityConfig
    {
        public void Configuration(IAppBuilder app)
        {
            app.CreatePerOwinContext(() => new TelimenaContext());
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