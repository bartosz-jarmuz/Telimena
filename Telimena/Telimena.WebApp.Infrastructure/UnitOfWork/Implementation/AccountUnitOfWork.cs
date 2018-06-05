using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telimena.WebApp.Infrastructure.UnitOfWork.Implementation
{
    using Core.Interfaces;
    using Core.Models;
    using Database;
    using Identity;
    using Microsoft.AspNet.Identity;
    using Microsoft.Owin.Security;
    using Repository.Implementation;

    public class AccountUnitOfWork : IAccountUnitOfWork
    {
        public AccountUnitOfWork(IAuthenticationManager authManager, ITelimenaUserManager userManager, TelimenaContext context)
        {
            this.AuthManager = authManager;
            this.UserManager = userManager;
            this.context = context;
            this.DeveloperRepository = new Repository<Developer>(context);
        }

        public IAuthenticationManager AuthManager { get; }
        public ITelimenaUserManager UserManager { get; }
        private readonly TelimenaContext context;
        public Repository<Developer> DeveloperRepository { get; }

        public async Task<Tuple<IdentityResult, IdentityResult>> RegisterUserAsync(TelimenaUser user, string password, params string[] roles)
        {
            IdentityResult registerResult = await this.UserManager.CreateAsync(user, password);
            if (registerResult.Succeeded)
            {
                TelimenaUser addedUser = await this.UserManager.FindByIdAsync(user.Id);

               IdentityResult roleResult = await this.HandleRoleRegistrationAsync(roles, addedUser);

                return new Tuple<IdentityResult, IdentityResult>(registerResult, roleResult);
            }
            else
            {
                return new Tuple<IdentityResult, IdentityResult>(registerResult, null);
            }
        }

        private async Task<IdentityResult> HandleRoleRegistrationAsync(string[] roles, TelimenaUser user)
        {
            IdentityResult roleresult = IdentityResult.Success;
            if (roles.Contains(TelimenaRoles.Developer))
            {
                roleresult = await this.UserManager.AddToRoleAsync(user.Id, TelimenaRoles.Viewer);
                if (!roleresult.Succeeded)
                {
                    return roleresult;
                }
            }
           

            if (roles.Contains(TelimenaRoles.Developer))
            {
                roleresult = await this.UserManager.AddToRoleAsync(user.Id, TelimenaRoles.Developer);
                if (roleresult.Succeeded)
                {
                    var developer = new Core.Models.Developer()
                    {
                        MainUser = user,
                        Name = user.DisplayName,
                        MainEmail = user.Email
                    };
                    this.DeveloperRepository.Add(developer);
                }
            }
            return roleresult;
        }

        public Task CompleteAsync()
        {
            return this.context.SaveChangesAsync();
        }
    }
}
