namespace Telimena.WebApp.Infrastructure.UnitOfWork.Implementation
{
    using System.Threading.Tasks;
    using Database;
    using Repository;
    using Repository.Implementation;

    public class StatisticsUnitOfWork : IStatisticsUnitOfWork
    {
        private readonly TelimenaContext context;

        public StatisticsUnitOfWork(TelimenaContext context)
        {
            this.context = context;
            this.ClientAppUserRepository = new ClientAppUserRepository(context);
            this.FunctionRepository= new FunctionRepository(context);
            this.ProgramRepository= new ProgramRepository(context);
        }

        public IClientAppUserRepository ClientAppUserRepository { get; }
        public IProgramRepository ProgramRepository { get; }
        public IFunctionRepository FunctionRepository { get; }
        public async Task CompleteAsync()
        {
            await this.context.SaveChangesAsync();
        }
    }
}