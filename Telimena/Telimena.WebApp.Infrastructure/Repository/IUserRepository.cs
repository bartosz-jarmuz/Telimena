using System.Security.Principal;
using System.Threading.Tasks;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Portal;

namespace Telimena.WebApp.Infrastructure.Repository
{
    public interface IUserRepository :IRepository<TelimenaUser>
    {
        Task<TelimenaUser> GetByPrincipalAsync(IPrincipal userPrincipal);
        TelimenaUser GetByPrincipal(IPrincipal userPrincipal);
    }
}