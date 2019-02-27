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
using Telimena.WebApp.Core.Models.Portal;
using Telimena.WebApp.Infrastructure.Database;

namespace Telimena.WebApp.Infrastructure.Repository.Implementation
{
    #region Using

    #endregion

    internal class ProgramRepository : IProgramRepository
    {
        public ProgramRepository(TelimenaPortalContext portalContext)
        {
            this.portalContext = portalContext;
        }

        private readonly TelimenaPortalContext portalContext;


        public void Add(Program objectToAdd)
        {
            if (objectToAdd.Updater == null)
            {
                objectToAdd.Updater = this.portalContext.Updaters.FirstOrDefault(x => x.InternalName == DefaultToolkitNames.UpdaterInternalName);
            }
            this.portalContext.Programs.Add(objectToAdd);
        }

        public async Task<Program> FirstOrDefaultAsync(Expression<Func<Program, bool>> predicate = null)
        {
            Program prg;
            if (predicate == null)
            {
                prg = await this.portalContext.Set<Program>().FirstOrDefaultAsync().ConfigureAwait(false);
            }
            else
            {
                prg = await this.portalContext.Set<Program>().FirstOrDefaultAsync(predicate).ConfigureAwait(false);
            }

            if (prg != null && prg.Updater == null)
            {
                prg.Updater = await this.portalContext.Updaters.FirstOrDefaultAsync(x => x.InternalName == DefaultToolkitNames.UpdaterInternalName).ConfigureAwait(false);
            }

            return prg;

        }



        public async Task<Program> SingleOrDefaultAsync(Expression<Func<Program, bool>> predicate = null)
        {
            Program prg; 
            if (predicate == null)
            {
                prg = await this.portalContext.Set<Program>().SingleOrDefaultAsync().ConfigureAwait(false);
            }
            else
            {
                prg = await this.portalContext.Set<Program>().SingleOrDefaultAsync(predicate).ConfigureAwait(false);
            }

            if (prg != null && prg.Updater == null)
            {
                prg.Updater = await this.portalContext.Updaters.FirstOrDefaultAsync(x => x.InternalName == DefaultToolkitNames.UpdaterInternalName).ConfigureAwait(false);
            }

            return prg;
        }
        public void Remove(Program program)
        {
            //this.telemetryContext.ViewTelemetryDetails.RemoveRange(this.telemetryContext.ViewTelemetryDetails.Where(x => x.TelemetrySummary.View.ProgramId == program.Id));
            //this.telemetryContext.ViewTelemetrySummaries.RemoveRange(this.telemetryContext.ViewTelemetrySummaries.Where(x => x.View.ProgramId == program.Id));
            //this.telemetryContext.Views.RemoveRange(this.telemetryContext.Views.Where(x => x.ProgramId == program.Id));

            this.portalContext.Versions.RemoveRange(this.portalContext.Versions.Where(x => x.ProgramAssembly.ProgramId == program.Id));
            this.portalContext.ProgramAssemblies.RemoveRange(this.portalContext.ProgramAssemblies.Where(x => x.ProgramId == program.Id));

            this.portalContext.ProgramPackages.RemoveRange(this.portalContext.ProgramPackages.Where(x => x.ProgramId == program.Id));

            this.portalContext.Programs.Remove(program);

        }

        public async Task<IEnumerable<Program>> GetAsync(Expression<Func<Program, bool>> filter = null
            , Func<IQueryable<Program>, IOrderedQueryable<Program>> orderBy = null, string includeProperties = "")
        {
            IQueryable<Program> query = Repository<Program>.PrepareQuery(this.portalContext, filter, includeProperties);

            if (orderBy != null)
            {
                return await orderBy(query).ToListAsync().ConfigureAwait(false);
            }

            return await query.ToListAsync().ConfigureAwait(false);
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
                return this.portalContext.Programs;
            }

            return this.portalContext.Programs.Where(x => x.DeveloperTeam != null && x.DeveloperTeam.MainUserId == user.Id);
        }
    }
}