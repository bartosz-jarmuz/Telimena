using System.Threading.Tasks;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Identity;
using Telimena.WebApp.Infrastructure.Repository;

namespace Telimena.WebApp.Infrastructure.UnitOfWork
{
    public interface IProgramsUnitOfWork
    {
        IRepository<AssemblyVersion> Versions { get; }
        IRepository<TelimenaUser> Users { get; }
        ITelimenaUserManager TelimenaUserManager { get; set; }

        IProgramRepository Programs { get; }

        IFunctionRepository Functions { get; }
        IToolkitDataRepository ToolkitData { get; set; }
        IUpdatePackageRepository UpdatePackages { get; set; }
        IProgramPackageRepository ProgramPackages { get; set; }
        Task CompleteAsync();
    }
}