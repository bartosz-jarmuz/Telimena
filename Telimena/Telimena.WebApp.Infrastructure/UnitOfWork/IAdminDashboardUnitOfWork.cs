namespace Telimena.WebApp.Infrastructure.Repository
{
    using System.Threading.Tasks;
    using Core.DTO;

    public interface IAdminDashboardUnitOfWork
    {
        Task<PortalSummaryData> GetPortalSummary();
        Task<AllProgramsSummaryData> GetAllProgramsSummaryCounts();
    }
}