using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Migrations;
using TelimenaClient;

namespace Telimena.WebApp.Infrastructure.Database
{
    public class TelimenaDbInitializer : MigrateDatabaseToLatestVersion<TelimenaContext, Configuration>
    {
        public static void SeedUsers(TelimenaContext context)
        {
            RoleStore<IdentityRole> store = new RoleStore<IdentityRole>(context);
            RoleManager<IdentityRole> manager = new RoleManager<IdentityRole>(store);

            CreateRole(manager, TelimenaRoles.Admin, context);
            CreateRole(manager, TelimenaRoles.Developer, context);
            CreateRole(manager, TelimenaRoles.Viewer, context);

            CreateAdmin(context);
            CreateDeveloper(context);
        }

        public static void SeedToolkit(TelimenaContext context)
        {
            if (!context.Updaters.Any(x => x.InternalName == DefaultToolkitNames.UpdaterInternalName))
            {
                var updater = new Updater(DefaultToolkitNames.UpdaterFileName, DefaultToolkitNames.UpdaterInternalName);
                context.Updaters.AddOrUpdate(updater);
            }
       
        }

    

        private static void CreateAdmin(TelimenaContext context)
        {
            if (!context.Users.Any(u => u.UserName == "superuser"))
            {
                UserStore<TelimenaUser> userStore = new UserStore<TelimenaUser>(context);
                UserManager<TelimenaUser> userManager = new UserManager<TelimenaUser>(userStore);
                TelimenaUser user = new TelimenaUser("superuser", "Super User")
                {
                    IsActivated = true,
#if DEBUG
                   MustChangePassword = false
#else
                   MustChangePassword = true
#endif

                };

                userManager.Create(user, "123456");
                userManager.AddToRole(user.Id, TelimenaRoles.Admin);

                var developer = new DeveloperAccount(user);
                developer.Name = DefaultToolkitNames.TelimenaSystemDevTeam;
                context.Developers.AddOrUpdate(developer);

              
            }
        }



        private static void CreateDeveloper(TelimenaContext context)
        {
#if DEBUG
            if (!context.Users.Any(u => u.UserName == "test@teli.mena"))
            {
                UserStore<TelimenaUser> userStore = new UserStore<TelimenaUser>(context);
                UserManager<TelimenaUser> userManager = new UserManager<TelimenaUser>(userStore);
                TelimenaUser user = new TelimenaUser("test@teli.mena", "Telimena Test Dev") {IsActivated = true};

                userManager.Create(user, "123456");
                userManager.AddToRole(user.Id, TelimenaRoles.Developer);

                DeveloperAccount developer = new DeveloperAccount(user);
                context.Developers.AddOrUpdate(developer);
                context.Users.Attach(user);
            }
#endif
        }

        private static void CreateRole(RoleManager<IdentityRole> manager, string roleName, TelimenaContext context)
        {
            if (!context.Roles.Any(r => r.Name == roleName))
            {
                IdentityRole role = new IdentityRole {Name = roleName};
                manager.Create(role);
            }
        }
    }
}