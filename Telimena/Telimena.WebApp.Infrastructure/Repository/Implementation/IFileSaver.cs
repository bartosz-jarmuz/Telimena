namespace Telimena.WebApp.Infrastructure.Repository.Implementation
{
    using System.IO;
    using System.Threading.Tasks;
    using Core.Models;

    public interface IFileSaver
    {
        Task SaveFile(IRepositoryFile file, Stream fileStream);
    }
}