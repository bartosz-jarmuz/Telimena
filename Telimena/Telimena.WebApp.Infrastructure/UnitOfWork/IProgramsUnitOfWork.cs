using System.Threading.Tasks;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Identity;
using Telimena.WebApp.Infrastructure.Repository;

namespace Telimena.WebApp.Infrastructure.UnitOfWork
{
    public interface IProgramsUnitOfWork
    {
        IRepository<AssemblyVersionInfo> Versions { get; }
        IUserRepository Users { get; }
        ITelimenaUserManager TelimenaUserManager { get; set; }

        IProgramRepository Programs { get; }

        IRepository<View> Views { get; }
        IToolkitDataRepository ToolkitData { get; set; }
        IUpdatePackageRepository UpdatePackages { get; set; }
        IUpdaterRepository UpdaterRepository { get; set; }
        IProgramPackageRepository ProgramPackages { get; set; }
        Task CompleteAsync();
    }
}