using System.Threading.Tasks;

namespace Telimena.WebApp.Infrastructure.UnitOfWork
{
    using Core.Models;
    using Repository;

    public interface IToolkitDataUnitOfWork
    {
        IUpdaterRepository UpdaterRepository { get; }
        IToolkitDataRepository ToolkitDataRepository { get; }
        Task CompleteAsync();
    }
}
