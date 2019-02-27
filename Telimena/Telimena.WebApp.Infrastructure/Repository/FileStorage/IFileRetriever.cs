using System.Threading.Tasks;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Portal;

namespace Telimena.WebApp.Infrastructure.Repository.FileStorage
{
    public interface IFileRetriever
    {
        Task<byte[]> GetFile(IRepositoryFile file, string containerName);
    }
}