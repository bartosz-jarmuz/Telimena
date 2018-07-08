namespace Telimena.WebApp.Infrastructure.UnitOfWork.Implementation
{
    using System.Threading.Tasks;
    using Core.Models;
    using Database;
    using Identity;
    using Repository;
    using Repository.Implementation;

    public class ProgramsUnitOfWork : IProgramsUnitOfWork
    {
        private readonly TelimenaContext context;


        public ProgramsUnitOfWork(TelimenaContext context, ITelimenaUserManager userManager)
        {
            this.context = context;
            this.Versions = new Repository<AssemblyVersion>(context);
            this.Users = new Repository<TelimenaUser>(context);
            this.Functions = new FunctionRepository(context);
            this.Programs = new ProgramRepository(context);
            this.TelimenaToolkitData = new TelimenaToolkitDataRepository(context);
            this.UpdatePackages = new UpdatePackageRepository(context);
            this.TelimenaUserManager = userManager;
        }

        public IRepository<TelimenaUser> Users { get; }
        public ITelimenaUserManager TelimenaUserManager { get; set; }
        public ITelimenaToolkitDataRepository TelimenaToolkitData { get; set; }
        public IUpdatePackageRepository UpdatePackages { get; set; }
        public IRepository<AssemblyVersion> Versions { get; }
        public IProgramRepository Programs { get; }
        public IFunctionRepository Functions { get; }
        public async Task CompleteAsync()
        {
            await this.context.SaveChangesAsync();
        }
    }
}