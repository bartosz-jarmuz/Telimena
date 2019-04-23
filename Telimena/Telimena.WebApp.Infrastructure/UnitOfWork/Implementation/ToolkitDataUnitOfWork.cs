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

        public ToolkitDataUnitOfWork(TelimenaPortalContext context, IAssemblyStreamVersionReader versionReader)
        {
            this.context = context;
            this.UpdaterRepository = new UpdaterRepository(context,versionReader);
            this.Programs = new ProgramRepository(context);
            this.Users = new UserRepository(context);
            this.ToolkitDataRepository = new ToolkitDataRepository(context, versionReader);
        }

        public IProgramRepository Programs { get; set; }

        public IUserRepository Users { get;  }

        private readonly TelimenaPortalContext context;

        public IUpdaterRepository UpdaterRepository { get; }
        public IToolkitDataRepository ToolkitDataRepository { get; }

        public async Task CompleteAsync()
        {
            await this.context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}