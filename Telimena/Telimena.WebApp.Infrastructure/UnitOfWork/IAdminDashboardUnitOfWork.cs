namespace Telimena.WebApp.Infrastructure.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Core.DTO;
    using Core.Models;

    public interface IAdminDashboardUnitOfWork
    {
        IProgramRepository Programs { get; }
        Task<PortalSummaryData> GetPortalSummary();
        Task<AllProgramsSummaryData> GetAllProgramsSummaryCounts();
        Task<IEnumerable<ProgramSummary>> GetProgramsSummary();
    }
}