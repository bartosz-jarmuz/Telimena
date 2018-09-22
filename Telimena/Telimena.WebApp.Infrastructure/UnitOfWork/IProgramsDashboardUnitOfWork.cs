using System.Collections.Generic;
using System.Threading.Tasks;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.Models;

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
    }
}