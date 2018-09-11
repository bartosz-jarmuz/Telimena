namespace Telimena.WebApp.Infrastructure.UnitOfWork.Implementation
{
    using System.Threading.Tasks;
    using Core.Models;
    using Database;
    using Repository;
    using Repository.Implementation;

    public class StatisticsUnitOfWork : IStatisticsUnitOfWork
    {
        private readonly TelimenaContext context;

        internal StatisticsUnitOfWork() : this(new TelimenaContext())
        {
        }

        public StatisticsUnitOfWork(TelimenaContext context)
        {
            this.context = context;
            this.ClientAppUsers = new Repository<ClientAppUser>(context);
            this.Versions = new Repository<AssemblyVersion>(context);
            this.Developers = new Repository<DeveloperAccount>(context);
            this.Functions= new FunctionRepository(context);
            this.Programs= new ProgramRepository(context);
            this.ToolkitData = new ToolkitDataRepository(context);
        }

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