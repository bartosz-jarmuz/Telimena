using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Threading.Tasks;
using Telimena.WebApp.Core.Models;

namespace Telimena.WebApp.Infrastructure.Repository
{
    public interface IProgramRepository 
    {
        List<Program> GetProgramsVisibleToUser(TelimenaUser user, IPrincipal principal);
        Task<List<Program>> GetProgramsVisibleToUserAsync(TelimenaUser user, IPrincipal principal);
        void Add(Program objectToAdd);
        Task<Program> FirstOrDefaultAsync(Expression<Func<Program, bool>> predicate = null);
        Task<Program> SingleOrDefaultAsync(Expression<Func<Program, bool>> predicate = null);
        void Remove(Program program);

        Task<IEnumerable<Program>> GetAsync(Expression<Func<Program, bool>> filter = null
            , Func<IQueryable<Program>, IOrderedQueryable<Program>> orderBy = null, string includeProperties = "");
    }
}