using System;
using System.Data.Entity;
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
            this.Programs = new ProgramRepository(portalContext);
            this.ToolkitData = new ToolkitDataRepository(telemetryContext, versionReader);
        }

        private ProgramRepository Programs { get; set; }
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

        public async Task<Program> GetProgramFirstOrDefault(Expression<Func<Program, bool>> predicate)
        {
            return await this.Programs.FirstOrDefaultAsync(predicate).ConfigureAwait(false);
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

    public class RegisterProgramUnitOfWork : IRegisterProgramUnitOfWork
    {
        private TelimenaPortalContext portalContext;
        private TelimenaTelemetryContext telemetryContext;

        public RegisterProgramUnitOfWork(TelimenaTelemetryContext telemetryContext, TelimenaPortalContext portalContext)
        {
            this.telemetryContext = telemetryContext;
            this.portalContext = portalContext;
            this.Users = new UserRepository(portalContext);
            this.Programs = new ProgramRepository(portalContext);
        }

        public IUserRepository Users { get; }
        public IProgramRepository Programs { get; }

        /// <summary>
        /// Handles adding program to both databases and commits transaction
        /// </summary>
        /// <param name="developerTeam">The developer team.</param>
        /// <param name="program">The program.</param>
        /// <param name="primaryAss">The primary ass.</param>
        /// <returns>Task.</returns>
        public async Task RegisterProgram(DeveloperTeam developerTeam , Program program, ProgramAssembly primaryAss)
        {

            using (var dbContextTransaction = this.portalContext.Database.BeginTransaction())
            {
                try
                {
                    developerTeam.AddProgram(program);

                    program.PrimaryAssembly = primaryAss;
                    this.portalContext.Programs.Add(program);

                    await this.portalContext.SaveChangesAsync().ConfigureAwait(false);

                    var savedPrg = await this.portalContext.Programs.FirstOrDefaultAsync(x => x.PublicId == program.PublicId).ConfigureAwait(false);

                    var teleRoot = new TelemetryRootObject()
                    {
                        ProgramId = savedPrg.Id,
                        TelemetryKey = savedPrg.TelemetryKey
                    };


                    this.telemetryContext.TelemetryRootObjects.Add(teleRoot);

                    await this.telemetryContext.SaveChangesAsync().ConfigureAwait(false);

                    dbContextTransaction.Commit();
                }
                catch (Exception)
                {
                    dbContextTransaction.Rollback();
                    throw;
                }
            }

            
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