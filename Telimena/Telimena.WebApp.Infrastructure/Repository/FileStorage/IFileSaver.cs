using System.IO;
using System.Threading.Tasks;
using Telimena.WebApp.Core.Models;

namespace Telimena.WebApp.Infrastructure.Repository.FileStorage
{
    public interface IFileSaver
    {
        Task SaveFile(IRepositoryFile file, Stream fileStream, string containerName);
    }
}