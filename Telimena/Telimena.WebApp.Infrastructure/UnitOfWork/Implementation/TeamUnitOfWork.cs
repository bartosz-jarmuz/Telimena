using System.Threading.Tasks;
using Telimena.WebApp.Core.Models.Portal;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Repository;
using Telimena.WebApp.Infrastructure.Repository.Implementation;

namespace Telimena.WebApp.Infrastructure.UnitOfWork.Implementation
{
    public class TeamUnitOfWork : ITeamUnitOfWork
    {
        private readonly TelimenaPortalContext context;

        public TeamUnitOfWork(TelimenaPortalContext context)
        {
            this.context = context;
            this.Developers = new Repository<DeveloperTeam>(context);
            this.Users = new Repository<TelimenaUser>(this.context);
        }

        public IRepository<TelimenaUser> Users { get; set; }

        public IRepository<DeveloperTeam> Developers { get; set; }

        public Task CompleteAsync()
        {
            return this.context.SaveChangesAsync();
        }
    }
}