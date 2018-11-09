using System.Threading.Tasks;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Identity;
using Telimena.WebApp.Infrastructure.Repository;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;
using Telimena.WebApp.Infrastructure.Repository.Implementation;

namespace Telimena.WebApp.Infrastructure.UnitOfWork.Implementation
{
    public class ProgramsUnitOfWork : IProgramsUnitOfWork
    {
        public ProgramsUnitOfWork(TelimenaContext context, ITelimenaUserManager userManager, IAssemblyStreamVersionReader versionReader)
        {
            this._context = context;
            this.Versions = new Repository<AssemblyVersionInfo>(context);
            this.Users = new UserRepository(context);
            this.Functions = new FunctionRepository(context);
            this.Programs = new ProgramRepository(context);
            this.ToolkitData = new ToolkitDataRepository(context, versionReader);
            this.UpdatePackages = new UpdatePackageRepository(context, versionReader);
            this.ProgramPackages = new ProgramPackageRepository(context, versionReader);
            this.UpdaterRepository = new UpdaterRepository(context, versionReader);
            this.TelimenaUserManager = userManager;
        }

        private readonly TelimenaContext _context;
        public IUserRepository Users { get; }

        public ITelimenaUserManager TelimenaUserManager { get; set; }
        public IToolkitDataRepository ToolkitData { get; set; }
        public IUpdatePackageRepository UpdatePackages { get; set; }
        public IUpdaterRepository UpdaterRepository { get; set; }
        public IProgramPackageRepository ProgramPackages { get; set; }
        public IRepository<AssemblyVersionInfo> Versions { get; }
        public IProgramRepository Programs { get; }
        public IFunctionRepository Functions { get; }

        public async Task CompleteAsync()
        {
            await this._context.SaveChangesAsync();
        }
    }
}