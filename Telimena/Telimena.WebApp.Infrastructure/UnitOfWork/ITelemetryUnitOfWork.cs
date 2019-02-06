using System.Threading.Tasks;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Repository;

namespace Telimena.WebApp.Infrastructure.UnitOfWork
{
    public interface ITelemetryUnitOfWork
    {
        IRepository<ClientAppUser> ClientAppUsers { get; }
        IRepository<AssemblyVersionInfo> Versions { get; }

        IProgramRepository Programs { get; }

        IToolkitDataRepository ToolkitData { get; }

        IRepository<View> Views { get; }
        IRepository<Event> Events{ get; }
        IRepository<DeveloperAccount> Developers { get; set; }
        IRepository<ExceptionInfo> Exceptions { get; }
        IRepository<LogMessage> LogMessages { get; }
        Task CompleteAsync();
    }
}