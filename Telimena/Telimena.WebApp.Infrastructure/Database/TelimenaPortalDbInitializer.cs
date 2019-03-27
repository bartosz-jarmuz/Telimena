using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Telimena.WebApp.Core.DTO.MappableToClient;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Portal;
using Telimena.WebApp.Infrastructure.Migrations;

namespace Telimena.WebApp.Infrastructure.Database
{
    //public class TelimenaPortalDbInitializer : DropCreateDatabaseIfModelChanges<TelimenaPortalContext>
    public class TelimenaPortalDbInitializer : MigrateDatabaseToLatestVersion<TelimenaPortalContext, PortalMigrationsConfiguration>
    {
        public static void SeedUsers(TelimenaPortalContext portalContext)
        {
            RoleStore<IdentityRole> store = new RoleStore<IdentityRole>(portalContext);
            RoleManager<IdentityRole> manager = new RoleManager<IdentityRole>(store);

            CreateRole(manager, TelimenaRoles.Admin, portalContext);
            CreateRole(manager, TelimenaRoles.Developer, portalContext);
            CreateRole(manager, TelimenaRoles.Viewer, portalContext);

            CreateAdmin(portalContext);
        }

        public static void SeedToolkit(TelimenaPortalContext portalContext)
        {
            if (!portalContext.Updaters.Any(x => x.InternalName == DefaultToolkitNames.UpdaterInternalName))
            {
                var updater = new Updater(DefaultToolkitNames.UpdaterFileName, DefaultToolkitNames.UpdaterInternalName);
                updater.Description = DefaultToolkitNames.UpdaterDescription;
                updater.DeveloperTeam = portalContext.Developers.FirstOrDefault(x => x.Name == DefaultToolkitNames.TelimenaSystemDevTeam);
                portalContext.Updaters.AddOrUpdate(updater);
            }
       
        }



        //protected override void Seed(TelimenaPortalContext portalContext)
        //{
        //    TelimenaPortalDbInitializer.SeedUsers(portalContext);
        //    TelimenaPortalDbInitializer.SeedToolkit(portalContext);
        //    portalContext.SaveChanges();

        //}

        private static void CreateAdmin(TelimenaPortalContext portalContext)
        {
            if (!portalContext.Users.Any(u => u.UserName == "superuser"))
            {
                UserStore<TelimenaUser> userStore = new UserStore<TelimenaUser>(portalContext);
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

                var developer = new DeveloperTeam(user);
                developer.Name = DefaultToolkitNames.TelimenaSystemDevTeam;
                portalContext.Developers.AddOrUpdate(developer);

              
            }
        }


        private static void CreateRole(RoleManager<IdentityRole> manager, string roleName, TelimenaPortalContext portalContext)
        {
            if (!portalContext.Roles.Any(r => r.Name == roleName))
            {
                IdentityRole role = new IdentityRole {Name = roleName};
                manager.Create(role);
            }
        }
    }
}