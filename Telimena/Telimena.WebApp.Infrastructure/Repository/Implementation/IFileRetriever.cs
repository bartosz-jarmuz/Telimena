using System.Threading.Tasks;
using Telimena.WebApp.Core.Models;

namespace Telimena.WebApp.Infrastructure.Repository.Implementation
{
    public interface IFileRetriever
    {
        Task<byte[]> GetFile(IRepositoryFile file);
    }
}