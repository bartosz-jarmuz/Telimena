using System.Threading.Tasks;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Repository;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;
using Telimena.WebApp.Infrastructure.Repository.Implementation;

namespace Telimena.WebApp.Infrastructure.UnitOfWork.Implementation
{
    public class StatisticsUnitOfWork : IStatisticsUnitOfWork
    {
      
        public StatisticsUnitOfWork(TelimenaContext context, IAssemblyVersionReader versionReader)
        {
            this.context = context;
            this.ClientAppUsers = new Repository<ClientAppUser>(context);
            this.Versions = new Repository<AssemblyVersion>(context);
            this.Developers = new Repository<DeveloperAccount>(context);
            this.Functions = new FunctionRepository(context);
            this.Programs = new ProgramRepository(context);
            this.ToolkitData = new ToolkitDataRepository(context, versionReader);
        }

        private readonly TelimenaContext context;

        public IRepository<DeveloperAccount> Developers { get; set; }
        public IRepository<AssemblyVersion> Versions { get; }
        public IRepository<ClientAppUser> ClientAppUsers { get; }
        public IProgramRepository Programs { get; }
        public IToolkitDataRepository ToolkitData { get; }
        public IFunctionRepository Functions { get; }

        public async Task CompleteAsync()
        {
            await this.context.SaveChangesAsync();
        }
    }
}