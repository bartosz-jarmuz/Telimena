namespace Telimena.WebApp.Infrastructure.UnitOfWork
{
    using System.Threading.Tasks;
    using Core.Models;
    using Identity;
    using Repository;

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