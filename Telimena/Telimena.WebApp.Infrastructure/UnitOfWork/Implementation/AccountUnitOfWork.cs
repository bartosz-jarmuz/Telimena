using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Identity;
using Telimena.WebApp.Infrastructure.Repository;
using Telimena.WebApp.Infrastructure.Repository.Implementation;

namespace Telimena.WebApp.Infrastructure.UnitOfWork.Implementation
{
    public class AccountUnitOfWork : IAccountUnitOfWork
    {
        public AccountUnitOfWork(IAuthenticationManager authManager, ITelimenaUserManager userManager, TelimenaContext context)
        {
            this.AuthManager = authManager;
            this.UserManager = userManager;
            this._context = context;
            this.DeveloperRepository = new Repository<DeveloperAccount>(context);
        }

        private readonly TelimenaContext _context;

        public IAuthenticationManager AuthManager { get; }
        public ITelimenaUserManager UserManager { get; }
        public IRepository<DeveloperAccount> DeveloperRepository { get; }

        public int Complete()
        {
            return this._context.SaveChanges();
        }

        public async Task<Tuple<IdentityResult, IdentityResult>> RegisterUserAsync(TelimenaUser user, string password, params string[] roles)
        {
            IdentityResult registerResult = await this.UserManager.CreateAsync(user, password);
            if (registerResult.Succeeded)
            {
                TelimenaUser addedUser = await this.UserManager.FindByIdAsync(user.Id);

                IdentityResult roleResult = await this.HandleRoleRegistrationAsync(roles, addedUser);

                return new Tuple<IdentityResult, IdentityResult>(registerResult, roleResult);
            }

            return new Tuple<IdentityResult, IdentityResult>(registerResult, null);
        }

        public Task CompleteAsync()
        {
            return this._context.SaveChangesAsync();
        }

        private async Task<IdentityResult> HandleRoleRegistrationAsync(string[] roles, TelimenaUser user)
        {
            IdentityResult roleresult = IdentityResult.Success;
            if (roles.Contains(TelimenaRoles.Viewer))
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
                    DeveloperAccount developer = new DeveloperAccount(user);
                    this.DeveloperRepository.Add(developer);
                    this._context.Users.Attach(user);
                }
            }

            return roleresult;
        }
    }
}