namespace Telimena.WebApp.Infrastructure.Repository
{
    using System.IO;
    using System.Threading.Tasks;
    using Core.Models;

    public interface IProgramPackageRepository
    {
        Task<ProgramPackageInfo> StorePackageAsync(int programId, Stream fileStream, string fileName);
        Task<byte[]> GetPackage(int packageId);
        Task<ProgramPackageInfo> GetPackageInfoForProgram(int programId);
    }
}