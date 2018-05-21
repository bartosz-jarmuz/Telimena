namespace Telimena.WebApp.Infrastructure.Repository
{
    using System.Threading.Tasks;
    using Core.DTO;

    public interface IAdminDashboardUnitOfWork
    {
        IProgramRepository Programs { get; }
        Task<PortalSummaryData> GetPortalSummary();
        Task<AllProgramsSummaryData> GetAllProgramsSummaryCounts();
    }
}