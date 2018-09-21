using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;
using Telimena.WebApp.Infrastructure.Repository.Implementation;

namespace Telimena.WebApp.Infrastructure.Repository
{
    public interface IToolkitDataRepository : IRepository<TelimenaToolkitData>
    {
        Task<byte[]> GetPackage(int toolkitDataId, IFileRetriever fileRetriever);
        Task<List<TelimenaPackageInfo>> GetPackagesNewerThan(string version);
        Task<TelimenaToolkitData> StorePackageAsync(string version, bool isBeta, bool introducesBreakingChanges, Stream fileStream, IFileSaver fileManager);
    }
}