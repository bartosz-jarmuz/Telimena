namespace Telimena.WebApp.Infrastructure.UnitOfWork.Implementation
{
    using System.Threading.Tasks;
    using Core.Models;
    using Database;
    using Repository;
    using Repository.Implementation;

    public class ProgramsUnitOfWork : IProgramsUnitOfWork
    {
        private readonly TelimenaContext context;

        internal ProgramsUnitOfWork() : this(new TelimenaContext())
        {
        }

        public ProgramsUnitOfWork(TelimenaContext context)
        {
            this.context = context;
            this.Versions = new Repository<AssemblyVersion>(context);
            this.Functions = new FunctionRepository(context);
            this.Programs = new ProgramRepository(context);
        }

        public IRepository<AssemblyVersion> Versions { get; }
        public IProgramRepository Programs { get; }
        public IFunctionRepository Functions { get; }
        public async Task CompleteAsync()
        {
            await this.context.SaveChangesAsync();
        }
    }
}