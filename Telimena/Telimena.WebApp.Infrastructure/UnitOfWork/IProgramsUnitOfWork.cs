using System.Threading.Tasks;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Portal;
using Telimena.WebApp.Core.Models.Telemetry;
using Telimena.WebApp.Infrastructure.Identity;
using Telimena.WebApp.Infrastructure.Repository;

namespace Telimena.WebApp.Infrastructure.UnitOfWork
{
    public interface IRegisterProgramUnitOfWork
    {
        IUserRepository Users { get; }
        IProgramRepository Programs { get; }

        /// <summary>
        /// Handles adding program to both databases and commits transaction
        /// </summary>
        /// <param name="developerTeam">The developer team.</param>
        /// <param name="program">The program.</param>
        /// <param name="primaryAss">The primary ass.</param>
        /// <returns>Task.</returns>
        Task RegisterProgram(DeveloperTeam developerTeam , Program program, ProgramAssembly primaryAss);
    }

    public interface IProgramsUnitOfWork
    {
        IRepository<AssemblyVersionInfo> Versions { get; }
        IUserRepository Users { get; }
        IProgramRepository Programs { get; }
        IToolkitDataRepository ToolkitData { get; set; }
        IUpdatePackageRepository UpdatePackages { get; set; }
        IUpdaterRepository UpdaterRepository { get; set; }
        IProgramPackageRepository ProgramPackages { get; set; }
        Task CompleteAsync();
    }
}