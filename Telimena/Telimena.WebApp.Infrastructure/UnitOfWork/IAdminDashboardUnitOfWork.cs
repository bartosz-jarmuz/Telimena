using System.Collections.Generic;
using System.Threading.Tasks;
using Telimena.WebApp.Core.DTO;

namespace Telimena.WebApp.Infrastructure.Repository
{
    public interface IAdminDashboardUnitOfWork
    {
        IProgramRepository Programs { get; }
        Task CompleteAsync();
        Task<AllProgramsSummaryData> GetAllProgramsSummaryCounts();
        Task<PortalSummaryData> GetPortalSummary();
        Task<IEnumerable<ProgramSummary>> GetProgramsSummary();
    }
}