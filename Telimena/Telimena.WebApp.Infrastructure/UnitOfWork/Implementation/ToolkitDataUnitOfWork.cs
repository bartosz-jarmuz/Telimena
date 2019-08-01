using System.Threading.Tasks;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Identity;
using Telimena.WebApp.Infrastructure.Repository;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;
using Telimena.WebApp.Infrastructure.Repository.Implementation;

namespace Telimena.WebApp.Infrastructure.UnitOfWork.Implementation
{
    public class ToolkitDataUnitOfWork : IToolkitDataUnitOfWork
    {

        public ToolkitDataUnitOfWork(TelimenaPortalContext portalContext, TelimenaTelemetryContext telemetryContext, IAssemblyStreamVersionReader versionReader)
        {
            this.portalContext = portalContext;
            this.telemetryContext = telemetryContext;
            this.UpdaterRepository = new UpdaterRepository(portalContext,versionReader);
            this.Programs = new ProgramRepository(portalContext, telemetryContext);
            this.Users = new UserRepository(portalContext);
            this.ToolkitDataRepository = new ToolkitDataRepository(portalContext, versionReader);
        }

        public IProgramRepository Programs { get; set; }

        public IUserRepository Users { get;  }

        private readonly TelimenaPortalContext portalContext;
        private readonly TelimenaTelemetryContext telemetryContext;

        public IUpdaterRepository UpdaterRepository { get; }
        public IToolkitDataRepository ToolkitDataRepository { get; }

        public async Task CompleteAsync()
        {
            await this.portalContext.SaveChangesAsync().ConfigureAwait(false);
            await this.telemetryContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}