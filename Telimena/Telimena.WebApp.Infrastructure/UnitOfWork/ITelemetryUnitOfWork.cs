using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Portal;
using Telimena.WebApp.Core.Models.Telemetry;
using Telimena.WebApp.Infrastructure.Repository;

namespace Telimena.WebApp.Infrastructure.UnitOfWork
{
    public interface ITelemetryUnitOfWork
    {
        IRepository<ClientAppUser> ClientAppUsers { get; }
        IRepository<AssemblyVersionInfo> Versions { get; }

        Task<TelemetryRootObject> GetMonitoredProgram(Guid telemetryKey);

        IToolkitDataRepository ToolkitData { get; }

        IRepository<View> Views { get; }
        IRepository<Event> Events{ get; }
        IRepository<ExceptionInfo> Exceptions { get; }
        IRepository<LogMessage> LogMessages { get; }
        Task CompleteAsync();
        void InsertMonitoredProgram(TelemetryRootObject program);
    }
}