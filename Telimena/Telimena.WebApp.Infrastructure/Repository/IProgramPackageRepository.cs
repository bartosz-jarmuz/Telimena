namespace Telimena.WebApp.Infrastructure.Repository
{
    using System.IO;
    using System.Threading.Tasks;
    using Core.Models;

    public interface IProgramPackageRepository : IRepository<ProgramPackageInfo>
    {
        Task<ProgramPackageInfo> StorePackageAsync(int programId, Stream fileStream, string fileName, string supportedToolkitVersion);
        Task<byte[]> GetPackage(int packageId);
        Task<ProgramPackageInfo> GetLatestProgramPackageInfo(int programId);
    }
}