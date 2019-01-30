using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Telimena.WebApp.Core;
using Telimena.WebApp.Core.DTO.MappableToClient;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;

namespace Telimena.WebApp.Infrastructure.Repository
{
    public interface IUpdatePackageRepository : IRepository<ProgramUpdatePackageInfo>
    {
        Task<List<ProgramUpdatePackageInfo>> GetAllPackages(int programId);
        Task<List<ProgramUpdatePackageInfo>> GetAllPackagesNewerThan(VersionData currentVersion, int programId);

        Task<byte[]> GetPackage(Guid packageId, IFileRetriever fileRetriever);
        Task<ProgramUpdatePackageInfo> GetUpdatePackageInfo(Guid id);
        Task<ProgramUpdatePackageInfo> StorePackageAsync(Program program, string packageName, Stream fileStream, string supportedToolkitVersion, IFileSaver fileSaver);
    }
}