namespace Telimena.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using WebApp.Core.Interfaces;
    using WebApp.Core.Models;
    using WebApp.Infrastructure.Identity;

    public class TelimenaDbInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<TelimenaContext>
    {
        protected override void Seed(TelimenaContext context)
        {
            TelimenaDbInitializer.SeedUsers(context);

            List<Developer> developers = new List<Developer>
            {
                new Developer {Name = "JimBeam"},
            };

            developers.ForEach(s => context.Developers.Add(s));
            context.SaveChanges();

            List<Program> programs = new List<Program>
            {
                new Program{Name="JimmyBeamyApp", DeveloperId = 1},
                new Program{Name="New JimmyBeamyApp", DeveloperId = 1},
            };

            programs.ForEach(s => context.Programs.Add(s));
            context.SaveChanges();
        }

        private static void SeedUsers(TelimenaContext context)
        {
            RoleStore<IdentityRole> store = new RoleStore<IdentityRole>(context);
            RoleManager<IdentityRole> manager = new RoleManager<IdentityRole>(store);
            
            TelimenaDbInitializer.CreateRole(manager, TelimenaRoles.Admin, context);
            TelimenaDbInitializer.CreateRole(manager, TelimenaRoles.Developer, context);
            TelimenaDbInitializer.CreateRole(manager, TelimenaRoles.Viewer, context);

            if (!context.Users.Any(u => u.UserName == "superuser"))
            {
               UserStore<TelimenaUser> userStore = new UserStore<TelimenaUser>(context);
               UserManager<TelimenaUser> userManager = new UserManager<TelimenaUser>(userStore);
               TelimenaUser  user = new TelimenaUser
                {
                    UserName = "superuser",
                    CreatedDate = DateTime.UtcNow,
                    DisplayName = "Super User",
                    IsActivated = true,
                    MustChangePassword = true
                };

                userManager.Create(user, "OneTimePassword");
                userManager.AddToRole(user.Id, TelimenaRoles.Admin);
            }
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