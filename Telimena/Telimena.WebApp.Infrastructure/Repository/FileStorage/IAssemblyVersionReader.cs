using System.IO;
using System.Threading.Tasks;

namespace Telimena.WebApp.Infrastructure.Repository.FileStorage
{
    public interface IAssemblyVersionReader
    {
        Task<string> GetFileVersion(Stream stream);
    }
}