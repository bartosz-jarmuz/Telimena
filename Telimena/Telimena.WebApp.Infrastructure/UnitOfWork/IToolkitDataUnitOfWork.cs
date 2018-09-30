using System.Threading.Tasks;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Identity;
using Telimena.WebApp.Infrastructure.Repository;

namespace Telimena.WebApp.Infrastructure.UnitOfWork
{
    public interface IToolkitDataUnitOfWork
    {
        IUpdaterRepository UpdaterRepository { get; }
        IToolkitDataRepository ToolkitDataRepository { get; }
        IUserRepository Users { get; }
        IProgramRepository Programs { get; set; }
        Task CompleteAsync();
    }
}