using System.IO;
using System.Threading.Tasks;
using Telimena.WebApp.Core.Models;

namespace Telimena.WebApp.Infrastructure.Repository
{
    public interface IProgramPackageRepository : IRepository<ProgramPackageInfo>
    {
        Task<ProgramPackageInfo> GetLatestProgramPackageInfo(int programId);
        Task<byte[]> GetPackage(int packageId);
        Task<ProgramPackageInfo> StorePackageAsync(int programId, Stream fileStream, string fileName, string supportedToolkitVersion);
    }
}