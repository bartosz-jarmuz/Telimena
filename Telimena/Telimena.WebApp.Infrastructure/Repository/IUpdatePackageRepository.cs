using System.Collections.Generic;
using System.Threading.Tasks;

namespace Telimena.WebApp.Infrastructure.Repository
{
    using System.IO;
    using Core.Models;

    public interface IUpdatePackageRepository : IRepository<ProgramUpdatePackageInfo>
    {
        Task<ProgramUpdatePackageInfo> StorePackageAsync(Program program, string version, Stream fileStream, string supportedToolkitVersion);

        Task<byte[]> GetPackage(int packageId);
        Task<List<ProgramUpdatePackageInfo>> GetAllPackages(int programId);
        Task<ProgramUpdatePackageInfo> GetUpdatePackageInfo(int id);
        Task<List<ProgramUpdatePackageInfo>> GetAllPackagesNewerThan(string currentVersion, int programId);
    }
}
