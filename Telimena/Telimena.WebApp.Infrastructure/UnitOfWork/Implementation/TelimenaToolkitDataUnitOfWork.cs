namespace Telimena.WebApp.Infrastructure.UnitOfWork.Implementation
{
    using System.Threading.Tasks;
    using Core.Models;
    using Database;
    using Repository;
    using Repository.Implementation;

    public class TelimenaToolkitDataUnitOfWork : ITelimenaToolkitDataUnitOfWork
    {
        private readonly TelimenaContext context;

        internal TelimenaToolkitDataUnitOfWork() : this(new TelimenaContext())
        {
        }

        public TelimenaToolkitDataUnitOfWork(TelimenaContext context)
        {
            this.context = context;
            this.UpdaterRepository = new UpdaterRepository(context, new LocalFileSaver(), new LocalFileRetriever());
            this.TelimenaToolkitDataRepository = new TelimenaToolkitDataRepository(context);
        }


        public IUpdaterRepository UpdaterRepository { get; }
        public ITelimenaToolkitDataRepository TelimenaToolkitDataRepository { get; }

        public async Task CompleteAsync()
        {
            await this.context.SaveChangesAsync();
        }
    }
}