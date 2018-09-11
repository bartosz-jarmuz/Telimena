using System.Collections.Generic;
using System.Threading.Tasks;
using Telimena.WebApp.Infrastructure.Repository.Implementation;

namespace Telimena.WebApp.Infrastructure.Repository
{
    using System.IO;
    using Core.Models;

    public interface IUpdaterRepository : IRepository<UpdaterPackageInfo>
    {
        Task<UpdaterPackageInfo> StorePackageAsync(string version, Stream fileStream, IFileSaver fileSaver);

        Task<byte[]> GetPackage(int packageId, IFileRetriever fileRetriever);
        Task<int> Save();

    }
}
