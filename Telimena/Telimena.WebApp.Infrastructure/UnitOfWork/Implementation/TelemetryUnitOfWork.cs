using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Telimena.WebApp.Core.Models;
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
            this.ClientAppUsers = new Repository<ClientAppUser>(telemetryContext);
            this.Exceptions= new Repository<ExceptionInfo>(telemetryContext);
            this.Versions = new Repository<AssemblyVersionInfo>(telemetryContext);
            this.Views = new ViewRepository(telemetryContext);
            this.Events = new Repository<Event>(telemetryContext);
            this.LogMessages = new Repository<LogMessage>(telemetryContext);
            this.MonitoredPrograms = new Repository<TelemetryMonitoredProgram>(telemetryContext);
            this.Programs = new ProgramRepository(portalContext);
            this.ToolkitData = new ToolkitDataRepository(telemetryContext, versionReader);
        }

        private ProgramRepository Programs { get; set; }
        private IRepository<TelemetryMonitoredProgram> MonitoredPrograms { get; }

        private readonly TelimenaTelemetryContext telemetryContext;

        public IRepository<Event> Events { get; }
        public IRepository<AssemblyVersionInfo> Versions { get; }
        public IRepository<ClientAppUser> ClientAppUsers { get; }

        public void InsertMonitoredProgram(TelemetryMonitoredProgram program)
        {
            this.MonitoredPrograms.Add(program);
        }

        public async Task<Program> GetProgramFirstOrDefault(Expression<Func<Program, bool>> predicate)
        {
            return await this.Programs.FirstOrDefaultAsync(predicate).ConfigureAwait(false);
        }

        public async Task<TelemetryMonitoredProgram> GetMonitoredProgram(Guid telemetryKey)
        {
            var monitoredPrg = await this.MonitoredPrograms.FirstOrDefaultAsync(x=>x.TelemetryKey == telemetryKey).ConfigureAwait(false);
            if (monitoredPrg != null)
            {
                return monitoredPrg;
            }
            else
            {
                var program = await this.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == telemetryKey)
                    .ConfigureAwait(false);
                if (program == null)
                {
                    return null;
                }
                else
                {
                    monitoredPrg = new TelemetryMonitoredProgram
                    {
                        ProgramId = program.Id,
                        TelemetryKey = program.TelemetryKey,
                    };
                    this.MonitoredPrograms.Add(monitoredPrg);
                    return monitoredPrg;
                }
            }
        }

        public IToolkitDataRepository ToolkitData { get; }
        public IRepository<View> Views { get; }
        public IRepository<ExceptionInfo> Exceptions { get; }
        public IRepository<LogMessage> LogMessages { get; }

        public async Task CompleteAsync()
        {
            await this.telemetryContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}