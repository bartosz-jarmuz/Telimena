using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Caching;
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
        public ProgramRepository(TelimenaPortalContext portalContext, TelimenaTelemetryContext telemetryContext)
        {
            this.portalContext = portalContext;
            this.telemetryContext = telemetryContext;
        }

        private readonly TelimenaPortalContext portalContext;
        private readonly TelimenaTelemetryContext telemetryContext;

        public void Add(Program objectToAdd)
        {
            if (objectToAdd.Updater == null)
            {
                objectToAdd.Updater = this.portalContext.Updaters.FirstOrDefault(x => x.InternalName == DefaultToolkitNames.UpdaterInternalName);
            }
            this.portalContext.Programs.Add(objectToAdd);
        }

        

        private async Task<Program> FirstOrDefaultAsync(Expression<Func<Program, bool>> predicate = null)
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

        public async Task<Program> GetByTelemetryKey(Guid telemetryKey)
        {
            return await this.FirstOrDefaultAsync(x => x.TelemetryKey == telemetryKey).ConfigureAwait(false);
        }

        public async Task<Program> GetByNames(string developerName, string programName)
        {
            return await this.FirstOrDefaultAsync(x => x.Name == programName && x.DeveloperTeam.Name == developerName).ConfigureAwait(false);
        }


        public async Task<Program> GetByProgramId(int id)
        {
            return await this.FirstOrDefaultAsync(x => x.Id == id).ConfigureAwait(false);
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
            this.telemetryContext.ViewTelemetryUnits.RemoveRange(this.telemetryContext.ViewTelemetryUnits.Where(x => x.TelemetryDetail.TelemetrySummary.View.ProgramId == program.Id));
            this.telemetryContext.ViewTelemetryDetails.RemoveRange(this.telemetryContext.ViewTelemetryDetails.Where(x => x.TelemetrySummary.View.ProgramId == program.Id));
            this.telemetryContext.ViewTelemetrySummaries.RemoveRange(this.telemetryContext.ViewTelemetrySummaries.Where(x => x.View.ProgramId == program.Id));
            this.telemetryContext.Views.RemoveRange(this.telemetryContext.Views.Where(x => x.ProgramId == program.Id));

            this.telemetryContext.EventTelemetryUnits.RemoveRange(this.telemetryContext.EventTelemetryUnits.Where(x => x.TelemetryDetail.TelemetrySummary.Event.ProgramId == program.Id));
            this.telemetryContext.EventTelemetryDetails.RemoveRange(this.telemetryContext.EventTelemetryDetails.Where(x => x.TelemetrySummary.Event.ProgramId == program.Id));
            this.telemetryContext.EventTelemetrySummaries.RemoveRange(this.telemetryContext.EventTelemetrySummaries.Where(x => x.Event.ProgramId == program.Id));
            this.telemetryContext.Events.RemoveRange(this.telemetryContext.Events.Where(x => x.ProgramId == program.Id));

            this.telemetryContext.LogMessages.RemoveRange(this.telemetryContext.LogMessages.Where(x => x.ProgramId == program.Id));
            this.telemetryContext.Exceptions.RemoveRange(this.telemetryContext.Exceptions.Where(x => x.ProgramId == program.Id));
            this.telemetryContext.TelemetryRootObjects.RemoveRange(this.telemetryContext.TelemetryRootObjects.Where(x => x.ProgramId == program.Id));

            this.portalContext.Versions.RemoveRange(this.portalContext.Versions.Where(x => x.ProgramAssembly.ProgramId == program.Id));
            this.portalContext.ProgramAssemblies.RemoveRange(this.portalContext.ProgramAssemblies.Where(x => x.ProgramId == program.Id));
            this.portalContext.UpdatePackages.RemoveRange(this.portalContext.UpdatePackages.Where(x => x.ProgramId == program.Id));

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

            return this.portalContext.Programs.Where(x => x.DeveloperTeam != null && (x.DeveloperTeam.MainUserId == user.Id || x.DeveloperTeam.AssociatedUsers.Any(a=>a.Id == user.Id)));
        }
    }
}