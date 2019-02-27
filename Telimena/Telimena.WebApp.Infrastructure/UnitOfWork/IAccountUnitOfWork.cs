using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Portal;
using Telimena.WebApp.Infrastructure.Identity;
using Telimena.WebApp.Infrastructure.Repository;

namespace Telimena.WebApp.Infrastructure.UnitOfWork.Implementation
{
    public interface IAccountUnitOfWork
    {
        IRepository<DeveloperTeam> DeveloperRepository { get; }
        IAuthenticationManager AuthManager { get; }
        ITelimenaUserManager UserManager { get; }
        int Complete();
        Task CompleteAsync();
        Task<Tuple<IdentityResult, IdentityResult>> RegisterUserAsync(TelimenaUser user, string password, params string[] roles);
    }
}