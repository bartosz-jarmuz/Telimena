using System.Threading.Tasks;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Portal;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Repository;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;
using Telimena.WebApp.Infrastructure.Repository.Implementation;

namespace Telimena.WebApp.Infrastructure.UnitOfWork.Implementation
{
    public class ProgramsUnitOfWork : IProgramsUnitOfWork
    {
        public ProgramsUnitOfWork(TelimenaPortalContext portalContext, TelimenaTelemetryContext telemetryContext, IAssemblyStreamVersionReader versionReader)
        {
            this.portalContext = portalContext;
            this.telemetryContext = telemetryContext;
            this.Versions = new Repository<AssemblyVersionInfo>(portalContext);
            this.Users = new UserRepository(portalContext);
            this.Programs = new ProgramRepository(portalContext, telemetryContext);
            this.ToolkitData = new ToolkitDataRepository(portalContext, versionReader);
            this.UpdatePackages = new UpdatePackageRepository(portalContext, versionReader);
            this.ProgramPackages = new ProgramPackageRepository(portalContext, versionReader);
            this.UpdaterRepository = new UpdaterRepository(portalContext, versionReader);
        }

        private readonly TelimenaPortalContext portalContext;
        private readonly TelimenaTelemetryContext telemetryContext;
        public IUserRepository Users { get; }

        public IToolkitDataRepository ToolkitData { get; set; }
        public IUpdatePackageRepository UpdatePackages { get; set; }
        public IUpdaterRepository UpdaterRepository { get; set; }
        public IProgramPackageRepository ProgramPackages { get; set; }
        public IRepository<AssemblyVersionInfo> Versions { get; }
        public IProgramRepository Programs { get; }

        public async Task CompleteAsync()
        {
            await this.portalContext.SaveChangesAsync().ConfigureAwait(false);
            await this.telemetryContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}