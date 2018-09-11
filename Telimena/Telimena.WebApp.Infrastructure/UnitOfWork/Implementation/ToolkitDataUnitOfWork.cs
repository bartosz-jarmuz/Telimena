namespace Telimena.WebApp.Infrastructure.UnitOfWork.Implementation
{
    using System.Threading.Tasks;
    using Core.Models;
    using Database;
    using Repository;
    using Repository.Implementation;

    public class ToolkitDataUnitOfWork : IToolkitDataUnitOfWork
    {
        private readonly TelimenaContext context;

        internal ToolkitDataUnitOfWork() : this(new TelimenaContext())
        {
        }

        public ToolkitDataUnitOfWork(TelimenaContext context)
        {
            this.context = context;
            this.UpdaterRepository = new UpdaterRepository(context);
            this.ToolkitDataRepository = new ToolkitDataRepository(context);
        }


        public IUpdaterRepository UpdaterRepository { get; }
        public IToolkitDataRepository ToolkitDataRepository { get; }

        public async Task CompleteAsync()
        {
            await this.context.SaveChangesAsync();
        }
    }
}