using System;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Portal;
using Telimena.WebApp.Core.Models.Telemetry;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Repository;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;
using Telimena.WebApp.Infrastructure.Repository.Implementation;

namespace Telimena.WebApp.Infrastructure.UnitOfWork.Implementation
{
    public class TelemetryUnitOfWork : ITelemetryUnitOfWork
    {
      
        public TelemetryUnitOfWork(TelimenaTelemetryContext telemetryContext, TelimenaPortalContext portalContext, IAssemblyStreamVersionReader versionReader)
        {
            this.telemetryContext = telemetryContext;
            this.portalContext = portalContext;
            this.ClientAppUsers = new Repository<ClientAppUser>(telemetryContext);
            this.Exceptions= new Repository<ExceptionInfo>(telemetryContext);
            this.Versions = new Repository<AssemblyVersionInfo>(telemetryContext);
            this.Views = new ViewRepository(telemetryContext);
            this.Events = new Repository<Event>(telemetryContext);
            this.LogMessages = new Repository<LogMessage>(telemetryContext);
            this.TelemetryRootObjects = new Repository<TelemetryRootObject>(telemetryContext);
            this.Programs = new ProgramRepository(portalContext, telemetryContext);
            this.ToolkitData = new ToolkitDataRepository(telemetryContext, versionReader);
        }

        internal ProgramRepository Programs { get; set; }
        private IRepository<TelemetryRootObject> TelemetryRootObjects { get; }

        private readonly TelimenaTelemetryContext telemetryContext;
        private readonly TelimenaPortalContext portalContext;

        public IRepository<Event> Events { get; }
        public IRepository<AssemblyVersionInfo> Versions { get; }
        public IRepository<ClientAppUser> ClientAppUsers { get; }

        public void InsertMonitoredProgram(TelemetryRootObject program)
        {
            this.TelemetryRootObjects.Add(program);
        }

        public Task<TelemetryRootObject> GetMonitoredProgram(Guid telemetryKey)
        {
            return ProgramEntitySynchronizer.GetRootObjectOrAddIfNotExists(this.telemetryContext, this.portalContext, telemetryKey);
        }

        public IToolkitDataRepository ToolkitData { get; }
        public IRepository<View> Views { get; }
        public IRepository<ExceptionInfo> Exceptions { get; }
        public IRepository<LogMessage> LogMessages { get; }

        public async Task CompleteAsync()
        {
            await this.telemetryContext.SaveChangesAsync().ConfigureAwait(false);
            await this.portalContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    public static class ProgramEntitySynchronizer
    {
        public static async Task<TelemetryRootObject> GetRootObjectOrAddIfNotExists(TelimenaTelemetryContext telemetryContext, TelimenaPortalContext portalContext, Guid telemetryKey)
        {
            var prg = await portalContext.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == telemetryKey).ConfigureAwait(false);
            if (prg == null)
            {
                return null;
            }

            var rootObject = await telemetryContext.TelemetryRootObjects.FirstOrDefaultAsync(x => x.ProgramId == prg.Id).ConfigureAwait(false);
            if (rootObject == null)
            {
                rootObject = new TelemetryRootObject
                {
                    ProgramId = prg.Id,
                    TelemetryKey = prg.TelemetryKey,
                };
                telemetryContext.TelemetryRootObjects.Add(rootObject);
                return rootObject;
            }

            return rootObject;
        }
    }
}