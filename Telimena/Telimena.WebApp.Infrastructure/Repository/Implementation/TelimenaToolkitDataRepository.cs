namespace Telimena.WebApp.Infrastructure.Repository.Implementation
{
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.Models;

    internal sealed class TelimenaToolkitDataRepository :  ITelimenaToolkitRepository
    {
        private readonly DbContext _dbContext;

        public TelimenaToolkitDataRepository(DbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public Task<TelimenaToolkitData> GetLatestToolkitData()
        {
            return this._dbContext.Set<TelimenaToolkitData>().OrderByDescending(x => x.Id).FirstOrDefaultAsync();
        }
    }
}