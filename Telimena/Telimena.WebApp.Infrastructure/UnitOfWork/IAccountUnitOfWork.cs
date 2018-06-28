namespace Telimena.WebApp.Infrastructure.UnitOfWork.Implementation
{
    using System;
    using System.Threading.Tasks;
    using Core.Models;
    using Identity;
    using Microsoft.AspNet.Identity;
    using Microsoft.Owin.Security;
    using Repository;
    using Repository.Implementation;

    public interface IAccountUnitOfWork
    {
        IRepository<DeveloperAccount> DeveloperRepository { get; }
        IAuthenticationManager AuthManager { get; }
        ITelimenaUserManager UserManager { get; }
        Task CompleteAsync();
        Task<Tuple<IdentityResult, IdentityResult>> RegisterUserAsync(TelimenaUser user, string password, params string[] roles);
    }
}