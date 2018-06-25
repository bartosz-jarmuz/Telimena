namespace Telimena.WebApp.Infrastructure.UnitOfWork
{
    using System.Threading.Tasks;
    using Core.Models;
    using Repository;

    public interface IProgramsUnitOfWork
    {
        IRepository<AssemblyVersion> Versions { get; }

        IProgramRepository Programs { get; }

        IFunctionRepository Functions { get; }

        Task CompleteAsync();
    }
}