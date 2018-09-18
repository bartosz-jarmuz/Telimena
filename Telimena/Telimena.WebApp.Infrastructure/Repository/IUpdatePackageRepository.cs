using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Telimena.WebApp.Core.Models;

namespace Telimena.WebApp.Infrastructure.Repository
{
    public interface IUpdatePackageRepository : IRepository<ProgramUpdatePackageInfo>
    {
        Task<List<ProgramUpdatePackageInfo>> GetAllPackages(int programId);
        Task<List<ProgramUpdatePackageInfo>> GetAllPackagesNewerThan(string currentVersion, int programId);

        Task<byte[]> GetPackage(int packageId);
        Task<ProgramUpdatePackageInfo> GetUpdatePackageInfo(int id);
        Task<ProgramUpdatePackageInfo> StorePackageAsync(Program program, string version, Stream fileStream, string supportedToolkitVersion);
    }
}