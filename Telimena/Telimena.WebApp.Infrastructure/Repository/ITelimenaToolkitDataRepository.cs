namespace Telimena.WebApp.Infrastructure.Repository
{
    using System.Threading.Tasks;
    using Core.Models;

    public interface ITelimenaToolkitDataRepository
    {
        Task<TelimenaToolkitData> GetLatestToolkitData();
    }
}