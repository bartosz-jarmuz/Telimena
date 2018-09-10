using System.Collections.Generic;
using System.Threading.Tasks;

namespace Telimena.WebApp.Infrastructure.Repository
{
    using System.IO;
    using Core.Models;

    public interface IUpdaterRepository : IRepository<UpdaterInfo>
    {
        Task<UpdaterInfo> StorePackageAsync(string version, Stream fileStream);

        Task<byte[]> GetPackage(int packageId);
        Task<int> Save();

    }
}
