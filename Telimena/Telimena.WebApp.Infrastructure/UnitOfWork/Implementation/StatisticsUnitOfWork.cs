namespace Telimena.WebApp.Infrastructure.UnitOfWork.Implementation
{
    using System.Threading.Tasks;
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
            this.ClientAppUserRepository = new ClientAppUserRepository(context);
            this.Functions= new FunctionRepository(context);
            this.Programs= new ProgramRepository(context);
        }

        public IClientAppUserRepository ClientAppUserRepository { get; }
        public IProgramRepository Programs { get; }
        public IFunctionRepository Functions { get; }
        public async Task CompleteAsync()
        {
            await this.context.SaveChangesAsync();
        }
    }
}