using System;
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

        Task<byte[]> GetPackage(Guid packageId, IFileRetriever fileRetriever);
        Task<ProgramUpdatePackageInfo> GetUpdatePackageInfo(Guid id);
        Task<ProgramUpdatePackageInfo> StorePackageAsync(Program program, string packageName, Stream fileStream, string supportedToolkitVersion, IFileSaver fileSaver);
    }
}