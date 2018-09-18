using System.Threading.Tasks;
using Telimena.WebApp.Infrastructure.Repository;

namespace Telimena.WebApp.Infrastructure.UnitOfWork
{
    public interface IToolkitDataUnitOfWork
    {
        IUpdaterRepository UpdaterRepository { get; }
        IToolkitDataRepository ToolkitDataRepository { get; }
        Task CompleteAsync();
    }
}