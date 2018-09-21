using System.IO;
using System.Threading.Tasks;
using System.Web.Hosting;
using Telimena.WebApp.Core.Models;

namespace Telimena.WebApp.Infrastructure.Repository.FileStorage
{
    public class LocalFileRetriever : IFileRetriever
    {
        public LocalFileRetriever()
        {
            this.RootFolder = HostingEnvironment.MapPath("~/App_Data/");
        }

        private string RootFolder { get; }

        public async Task<byte[]> GetFile(IRepositoryFile repositoryFile, string containerName)
        {
            byte[] result;
            using (FileStream stream = File.Open(repositoryFile.FileLocation, FileMode.Open))
            {
                result = new byte[stream.Length];
                await stream.ReadAsync(result, 0, (int) stream.Length);
            }

            return result;
        }
    }
}