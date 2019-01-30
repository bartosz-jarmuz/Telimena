using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Threading.Tasks;
using Telimena.WebApp.Core.DTO.MappableToClient;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Database;

namespace Telimena.WebApp.Infrastructure.Repository.Implementation
{
    #region Using

    #endregion

    internal class ProgramRepository : IProgramRepository
    {
        public ProgramRepository(DbContext dbContext) 
        {
            this.telimenaContext = dbContext as TelimenaContext;
        }

        private readonly TelimenaContext telimenaContext ;

        public void Add(Program objectToAdd)
        {
            if (objectToAdd.Updater == null)
            {
                objectToAdd.Updater = this.telimenaContext.Updaters.FirstOrDefault(x => x.InternalName == DefaultToolkitNames.UpdaterInternalName);
            }
            this.telimenaContext.Programs.Add(objectToAdd);
        }

        public async Task<Program> FirstOrDefaultAsync(Expression<Func<Program, bool>> predicate = null)
        {
            Program prg;
            if (predicate == null)
            {
                prg = await this.telimenaContext.Set<Program>().FirstOrDefaultAsync().ConfigureAwait(false);
            }
            else
            {
                prg = await this.telimenaContext.Set<Program>().FirstOrDefaultAsync(predicate).ConfigureAwait(false);
            }

            if (prg != null && prg.Updater == null)
            {
                prg.Updater = await this.telimenaContext.Updaters.FirstOrDefaultAsync(x => x.InternalName == DefaultToolkitNames.UpdaterInternalName).ConfigureAwait(false);
            }

            return prg;

        }



        public async Task<Program> SingleOrDefaultAsync(Expression<Func<Program, bool>> predicate = null)
        {
            Program prg; 
            if (predicate == null)
            {
                prg = await this.telimenaContext.Set<Program>().SingleOrDefaultAsync().ConfigureAwait(false);
            }
            else
            {
                prg = await this.telimenaContext.Set<Program>().SingleOrDefaultAsync(predicate).ConfigureAwait(false);
            }

            if (prg != null && prg.Updater == null)
            {
                prg.Updater = await this.telimenaContext.Updaters.FirstOrDefaultAsync(x => x.InternalName == DefaultToolkitNames.UpdaterInternalName).ConfigureAwait(false);
            }

            return prg;
        }
        public void Remove(Program program)
        {
            this.telimenaContext.ViewTelemetryDetails.RemoveRange(this.telimenaContext.ViewTelemetryDetails.Where(x => x.TelemetrySummary.View.ProgramId == program.Id));
            this.telimenaContext.ViewTelemetrySummaries.RemoveRange(this.telimenaContext.ViewTelemetrySummaries.Where(x => x.View.ProgramId == program.Id));
            this.telimenaContext.Views.RemoveRange(this.telimenaContext.Views.Where(x => x.ProgramId == program.Id));

            this.telimenaContext.Versions.RemoveRange(this.telimenaContext.Versions.Where(x => x.ProgramAssembly.ProgramId == program.Id));
            this.telimenaContext.ProgramAssemblies.RemoveRange(this.telimenaContext.ProgramAssemblies.Where(x => x.ProgramId == program.Id));

            this.telimenaContext.ProgramPackages.RemoveRange(this.telimenaContext.ProgramPackages.Where(x => x.ProgramId == program.Id));

            this.telimenaContext.Programs.Remove(program);

        }

        public async Task<IEnumerable<Program>> GetAsync(Expression<Func<Program, bool>> filter = null
            , Func<IQueryable<Program>, IOrderedQueryable<Program>> orderBy = null, string includeProperties = "")
        {
            IQueryable<Program> query = Repository<Program>.PrepareQuery(this.telimenaContext, filter, includeProperties);

            if (orderBy != null)
            {
                return await orderBy(query).ToListAsync().ConfigureAwait(false);
            }

            return await query.ToListAsync().ConfigureAwait(false);
        }

        

        public async Task<IEnumerable<Program>> GetProgramsByDeveloperName(string developerName)
        {
            return await this.telimenaContext.Programs.Include(x => x.DeveloperAccount)
                .Where(x => x.DeveloperAccount != null && x.DeveloperAccount.Name == developerName).ToListAsync().ConfigureAwait(false);
        }

        public async Task<IEnumerable<Program>> GetProgramsForUserAsync(TelimenaUser user)
        {
            return await this.telimenaContext.Programs.Where(x => x.DeveloperAccount != null && x.DeveloperAccount.MainUserId == user.Id).ToListAsync().ConfigureAwait(false);
        }

        public List<Program> GetProgramsVisibleToUser(TelimenaUser user, IPrincipal principal)
        {
            return this.GetProgramsVisibleToUserImpl(user, principal).ToList();
        }

        public Task<List<Program>> GetProgramsVisibleToUserAsync(TelimenaUser user, IPrincipal principal)
        {
            return this.GetProgramsVisibleToUserImpl(user, principal).ToListAsync();
        }

        private IQueryable<Program> GetProgramsVisibleToUserImpl(TelimenaUser user, IPrincipal principal)
        {
            if (principal != null && principal.IsInRole(TelimenaRoles.Admin))
            {
                return this.telimenaContext.Programs;
            }

            return this.telimenaContext.Programs.Where(x => x.DeveloperAccount != null && x.DeveloperAccount.MainUserId == user.Id);
        }
    }
}