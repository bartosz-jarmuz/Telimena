namespace Telimena.WebApp.Infrastructure.Database
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.Interfaces;
    using Core.Models;
    using Identity;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using UnitOfWork.Implementation;

    public class TelimenaDbInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<TelimenaContext>
    {


        protected override void Seed(TelimenaContext context)
        {
            TelimenaDbInitializer.SeedUsers(context);
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
#if RELEASE
                   MustChangePassword = true
#endif
                };

                userManager.Create(user, "123456");
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