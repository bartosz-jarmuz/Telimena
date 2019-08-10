using System;
using System.Threading.Tasks;
using Telimena.WebApp.Core.Models.Portal;

namespace Telimena.WebApp.Infrastructure.Repository.FileStorage
{
    public interface IFileRemover
    {
        Task DeleteFile(IRepositoryFile file, string containerName);
    }
}