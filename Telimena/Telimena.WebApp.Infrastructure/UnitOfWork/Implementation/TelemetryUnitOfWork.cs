using System.Threading.Tasks;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Repository;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;
using Telimena.WebApp.Infrastructure.Repository.Implementation;

namespace Telimena.WebApp.Infrastructure.UnitOfWork.Implementation
{
    public class TelemetryUnitOfWork : ITelemetryUnitOfWork
    {
      
        public TelemetryUnitOfWork(TelimenaContext context, IAssemblyStreamVersionReader versionReader)
        {
            this.context = context;
            this.ClientAppUsers = new Repository<ClientAppUser>(context);
            this.Exceptions= new Repository<ExceptionInfo>(context);
            this.Versions = new Repository<AssemblyVersionInfo>(context);
            this.Developers = new Repository<DeveloperAccount>(context);
            this.Views = new ViewRepository(context);
            this.Events = new Repository<Event>(context);
            this.LogMessages = new Repository<LogMessage>(context);
            this.Programs = new ProgramRepository(context);
            this.ToolkitData = new ToolkitDataRepository(context, versionReader);
        }

        private readonly TelimenaContext context;

        public IRepository<Event> Events { get; }
        public IRepository<DeveloperAccount> Developers { get; set; }
        public IRepository<AssemblyVersionInfo> Versions { get; }
        public IRepository<ClientAppUser> ClientAppUsers { get; }
        public IProgramRepository Programs { get; }
        public IToolkitDataRepository ToolkitData { get; }

        public IRepository<View> Views { get; }
        public IRepository<ExceptionInfo> Exceptions { get; }
        public IRepository<LogMessage> LogMessages { get; }

        public async Task CompleteAsync()
        {
            await this.context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}