using System.Threading.Tasks;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Repository;

namespace Telimena.WebApp.Infrastructure.UnitOfWork
{
    public interface IStatisticsUnitOfWork
    {
        IRepository<ClientAppUser> ClientAppUsers { get; }
        IRepository<AssemblyVersionInfo> Versions { get; }

        IProgramRepository Programs { get; }

        IToolkitDataRepository ToolkitData { get; }

        IViewRepository Views { get; }
        IRepository<DeveloperAccount> Developers { get; set; }
        Task CompleteAsync();
    }
}