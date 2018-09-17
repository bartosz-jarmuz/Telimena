using System.Collections.Generic;
using System.IO;
using Telimena.WebApp.Infrastructure.Repository.Implementation;

namespace Telimena.WebApp.Infrastructure.Repository
{
    using System.Threading.Tasks;
    using Core.Models;

    public interface IToolkitDataRepository :IRepository<TelimenaToolkitData>
    {
        Task<TelimenaToolkitData> StorePackageAsync(string version, Stream fileStream, IFileSaver fileSaver);
        Task<byte[]> GetPackage(int toolkitDataId, IFileRetriever fileRetriever);
        Task<List<TelimenaPackageInfo>> GetPackagesNewerThan(string version);
    }
}