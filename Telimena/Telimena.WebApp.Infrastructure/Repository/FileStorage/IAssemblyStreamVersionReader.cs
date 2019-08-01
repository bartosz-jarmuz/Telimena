using System.IO;
using System.Threading.Tasks;

namespace Telimena.WebApp.Infrastructure.Repository.FileStorage
{
    public interface IAssemblyStreamVersionReader
    {
        Task<string> GetFileVersion(Stream stream, string expectedFileName, bool expectSingleFile);
        Task<string> GetVersionFromPackage(string nameOfFileToCheck, Stream fileStream, string packageFileName
            , bool required = true);

        Task<string> GetEmbeddedAssemblyVersion(Stream stream, string expectedFileName, string expectedAssemblyName, bool expectSingleFile);
        Task<(string appVersion, string toolkitVersion)> GetVersionsFromStream(string uploadedFileName, Stream fileStream, string primaryAssemblyName);
    }
}