using System.IO;
using System.Threading.Tasks;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;
using Telimena.WebApp.Infrastructure.Repository.Implementation;

namespace Telimena.WebApp.Infrastructure.Repository
{
    public interface IUpdaterRepository : IRepository<UpdaterPackageInfo>
    {
        Task<UpdaterPackageInfo> GetNewestCompatibleUpdater(string version, string toolkitVersion, bool includingBeta);

        Task<byte[]> GetPackage(int packageId, IFileRetriever fileRetriever);
        Task<int> Save();
        Task<UpdaterPackageInfo> StorePackageAsync(string minimumRequiredToolkitVersion, Stream fileStream, IFileSaver fileSaver);
    }
}