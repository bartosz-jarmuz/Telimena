using System.Threading.Tasks;

namespace Telimena.WebApp.Infrastructure.UnitOfWork
{
    using Core.Models;
    using Repository;

    public interface ITelimenaToolkitDataUnitOfWork
    {
        IUpdaterRepository UpdaterRepository { get; }
        ITelimenaToolkitDataRepository TelimenaToolkitDataRepository { get; }
        Task CompleteAsync();
    }
}
