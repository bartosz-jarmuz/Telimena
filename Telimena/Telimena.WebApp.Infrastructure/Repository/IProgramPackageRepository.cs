using System.IO;
using System.Threading.Tasks;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;

namespace Telimena.WebApp.Infrastructure.Repository
{
    public interface IProgramPackageRepository : IRepository<ProgramPackageInfo>
    {
        Task<ProgramPackageInfo> GetLatestProgramPackageInfo(int programId);
        Task<byte[]> GetPackage(int packageId, IFileRetriever fileRetriever);
        Task<ProgramPackageInfo> StorePackageAsync(int programId, Stream fileStream, string fileName, IFileSaver fileManager);
    }
}