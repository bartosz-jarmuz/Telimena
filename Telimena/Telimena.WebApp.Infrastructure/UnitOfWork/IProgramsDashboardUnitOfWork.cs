using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using DataTables.AspNet.Core;
using Telimena.WebApp.Core;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.DTO.MappableToClient;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Portal;

namespace Telimena.WebApp.Infrastructure.Repository
{
    public interface IProgramsDashboardUnitOfWork
    {
        IRepository<TelimenaUser> Users { get; }

        IProgramRepository Programs { get; }
        Task CompleteAsync();

        Task<AllProgramsSummaryData> GetAllProgramsSummaryCounts(List<Program> programs);
        Task<PortalSummaryData> GetPortalSummary();
        Task<IEnumerable<ProgramUsageSummary>> GetProgramUsagesSummary(List<Program> programs);
        Task<UsageDataTableResult> GetProgramUsageData(Guid telemetryKey, TelemetryItemTypes itemType, int skip
            , int take, IEnumerable<Tuple<string, bool>> sortBy , ISearch requestSearch 
            , List<string> searchableColumns);
        Task<TelemetryInfoTable> GetPivotTableData(TelemetryItemTypes type, Guid telemetryKey);

        Task<UsageDataTableResult> GetExceptions(Guid telemetryKey, TelemetryItemTypes itemType, int skip, int take
            , IEnumerable<Tuple<string, bool>> sortBy , ISearch requestSearch             , List<string> searchableColumns);

        Task<UsageDataTableResult> GetLogs(Guid telemetryKey, int skip, int take
            , IEnumerable<Tuple<string, bool>> sortBy, string searchPhrase);

        Task<UsageDataTableResult> GetSequenceHistory(Guid telemetryKey, string sequenceId, string searchValue);

        Task<IEnumerable<ProgramSummary>> GetProgramSummary(List<Program> programs);
        Task<List<AppUsersSummaryData>> GetAppUsersSummary(List<Program> programs, DateTime? startDate, DateTime? endDate);
        Task<DataTable> GetDailyActivityScore(List<Program> programs, DateTime startDate, DateTime endDate);

        Task<DataTable> GetVersionDistribution(Program program, DateTime startDate, DateTime endDate);
        Task<DataTable> GetDailyUsersCount(List<Program> programs, DateTime startDate, DateTime endDate);
        Task<Dictionary<string, int>> GetEventNames(Program program);
        Task<Dictionary<string, int>> GetViewNames(Program program);
    }


}