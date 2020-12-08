using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Threading.Tasks;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Portal;

namespace Telimena.WebApp.Infrastructure.Repository
{
    public interface IProgramRepository 
    {
        List<Program> GetProgramsVisibleToUser(TelimenaUser user, IPrincipal principal);
        Task<List<Program>> GetProgramsVisibleToUserAsync(TelimenaUser user, IPrincipal principal);
        void Add(Program objectToAdd);
        Task<Program> SingleOrDefaultAsync(Expression<Func<Program, bool>> predicate = null);
        void Remove(Program program);

        Task<IEnumerable<Program>> GetAsync(Expression<Func<Program, bool>> filter = null
            , Func<IQueryable<Program>, IOrderedQueryable<Program>> orderBy = null, string includeProperties = "");

        Task<Program> GetByTelemetryKey(Guid telemetryKey);
        Task<Program> GetByProgramId(int id);
        Task<Program> GetByNames(string developerName, string programName);
        void ClearTelemetryData(Program program, TelemetryItemTypes? type);
    }
}