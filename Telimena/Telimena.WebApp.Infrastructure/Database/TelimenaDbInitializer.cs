namespace Telimena.WebApp.Infrastructure.Database
{
    using System.IO;
    using System.Linq;
    using Client;
    using Core.Interfaces;
    using Core.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;


    public class TelimenaDbInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<TelimenaContext>
    {
        protected override void Seed(TelimenaContext context)
        {
            TelimenaDbInitializer.SeedUsers(context);
            context.SaveChanges();
        }

        public static void SeedUsers(TelimenaContext context)
        {
            RoleStore<IdentityRole> store = new RoleStore<IdentityRole>(context);
            RoleManager<IdentityRole> manager = new RoleManager<IdentityRole>(store);
            
            TelimenaDbInitializer.CreateRole(manager, TelimenaRoles.Admin, context);
            TelimenaDbInitializer.CreateRole(manager, TelimenaRoles.Developer, context);
            TelimenaDbInitializer.CreateRole(manager, TelimenaRoles.Viewer, context);

            TelimenaDbInitializer.CreateAdmin(context);
            TelimenaDbInitializer.CreateDeveloper(context);
            SeedTelimenasAss(context);
        }

        private static void CreateDeveloper(TelimenaContext context)
        {
#if DEBUG
            if (!context.Users.Any(u => u.UserName == "test@teli.mena"))
            {
                UserStore<TelimenaUser> userStore = new UserStore<TelimenaUser>(context);
                UserManager<TelimenaUser> userManager = new UserManager<TelimenaUser>(userStore);
                TelimenaUser user = new TelimenaUser("test@teli.mena", "Telimena Test Dev")
                {
                    IsActivated = true,
                };

                userManager.Create(user, "123456");
                userManager.AddToRole(user.Id, TelimenaRoles.Developer);

                var developer = new Core.Models.DeveloperAccount(user);
                context.Developers.Add(developer);
                context.Users.Attach(user);
            }
#endif

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
#if RELEASE
                   MustChangePassword = true
#endif
                };

                userManager.Create(user, "123456");
                userManager.AddToRole(user.Id, TelimenaRoles.Admin);
            }
        }

        private static void SeedTelimenasAss(TelimenaContext context)
        {
            var ass = typeof(Telimena).Assembly;
            var clientData = new TelimenaToolkitData()
            {
                ReleaseDate = new FileInfo(ass.Location).LastWriteTimeUtc,
                Version = "1.0.0.0"
            };
            context.TelimenaToolkitData.Add(clientData);
            context.SaveChanges();
        }

        private static void CreateRole(RoleManager<IdentityRole> manager, string roleName, TelimenaContext context)
        {
            if (!context.Roles.Any(r => r.Name == roleName))
            {
                IdentityRole role = new IdentityRole {Name = roleName };
                manager.Create(role);
            }
        }
    }
}