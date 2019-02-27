using System;
using System.Collections.Generic;
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
        Task<IEnumerable<ProgramSummary>> GetProgramsSummary(List<Program> programs);
        Task<UsageDataTableResult> GetProgramUsageData(Guid telemetryKey, TelemetryItemTypes itemType, int skip
            , int take, IEnumerable<Tuple<string, bool>> sortBy , ISearch requestSearch 
            , List<string> searchableColumns);
        Task<TelemetryInfoTable> GetPivotTableData(TelemetryItemTypes type, Guid telemetryKey);

        Task<UsageDataTableResult> GetExceptions(Guid telemetryKey, TelemetryItemTypes itemType, int skip, int take
            , IEnumerable<Tuple<string, bool>> sortBy , ISearch requestSearch             , List<string> searchableColumns);

        Task<UsageDataTableResult> GetLogs(Guid telemetryKey, int skip, int take
            , IEnumerable<Tuple<string, bool>> sortBy, string searchPhrase);

        Task<UsageDataTableResult> GetSequenceHistory(Guid telemetryKey, string sequenceId, string searchValue);

    }


}