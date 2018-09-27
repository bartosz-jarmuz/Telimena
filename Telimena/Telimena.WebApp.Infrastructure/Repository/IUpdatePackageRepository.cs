using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;

namespace Telimena.WebApp.Infrastructure.Repository
{
    public interface IUpdatePackageRepository : IRepository<ProgramUpdatePackageInfo>
    {
        Task<List<ProgramUpdatePackageInfo>> GetAllPackages(int programId);
        Task<List<ProgramUpdatePackageInfo>> GetAllPackagesNewerThan(string currentVersion, int programId);

        Task<byte[]> GetPackage(int packageId, IFileRetriever fileRetriever);
        Task<ProgramUpdatePackageInfo> GetUpdatePackageInfo(int id);
        Task<ProgramUpdatePackageInfo> StorePackageAsync(Program program, Stream fileStream, string supportedToolkitVersion, IFileSaver fileSaver);
    }
}