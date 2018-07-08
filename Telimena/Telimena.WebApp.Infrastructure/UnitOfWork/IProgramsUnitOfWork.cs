namespace Telimena.WebApp.Infrastructure.UnitOfWork
{
    using System.Threading.Tasks;
    using Core.Models;
    using Identity;
    using Repository;
    using Repository.Implementation;

    public interface IProgramsUnitOfWork
    {
        IRepository<AssemblyVersion> Versions { get; }
        IRepository<TelimenaUser> Users { get; }
        ITelimenaUserManager TelimenaUserManager { get; set; }

        IProgramRepository Programs { get; }

        IFunctionRepository Functions { get; }
        ITelimenaToolkitDataRepository TelimenaToolkitData { get; set; }
        IUpdatePackageRepository UpdatePackages { get; set; }
        Task CompleteAsync();
    }
}