using System.Collections.Generic;
using System.Threading.Tasks;

namespace Telimena.WebApp.Infrastructure.Repository
{
    using System.IO;
    using Core.Models;

    public interface IUpdatePackageRepository : IRepository<UpdatePackageInfo>
    {
        Task<UpdatePackageInfo> StorePackageAsync(int programId, string version, Stream fileStream, string fileName);

        Task<byte[]> GetPackage(int packageId);
        Task<List<UpdatePackageInfo>> GetAllPackages(int programId);
        Task<UpdatePackageInfo> GetUpdatePackageInfo(int id);
        Task<List<UpdatePackageInfo>> GetAllPackagesNewerThan(int programId, string version);
    }
}
