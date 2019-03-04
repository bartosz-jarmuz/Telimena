using System.Threading.Tasks;
using Telimena.WebApp.Core.Models.Portal;
using Telimena.WebApp.Infrastructure.Repository;

namespace Telimena.WebApp.Infrastructure.UnitOfWork.Implementation
{
    public interface ITeamUnitOfWork
    {
        IRepository<TelimenaUser> Users { get; set; }
        IRepository<DeveloperTeam> Developers { get; set; }
        Task CompleteAsync();
    }
}