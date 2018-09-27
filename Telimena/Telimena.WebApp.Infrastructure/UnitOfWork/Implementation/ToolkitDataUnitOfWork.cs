using System.Threading.Tasks;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Repository;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;
using Telimena.WebApp.Infrastructure.Repository.Implementation;

namespace Telimena.WebApp.Infrastructure.UnitOfWork.Implementation
{
    public class ToolkitDataUnitOfWork : IToolkitDataUnitOfWork
    {
       
        public ToolkitDataUnitOfWork(TelimenaContext context, IAssemblyVersionReader versionReader)
        {
            this.context = context;
            this.UpdaterRepository = new UpdaterRepository(context,versionReader);
            this.ToolkitDataRepository = new ToolkitDataRepository(context, versionReader);
        }

        private readonly TelimenaContext context;

        public IUpdaterRepository UpdaterRepository { get; }
        public IToolkitDataRepository ToolkitDataRepository { get; }

        public async Task CompleteAsync()
        {
            await this.context.SaveChangesAsync();
        }
    }
}