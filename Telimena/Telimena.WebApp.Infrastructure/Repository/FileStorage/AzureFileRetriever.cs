using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using Telimena.WebApp.Core.Models;

namespace Telimena.WebApp.Infrastructure.Repository.FileStorage
{
    public class AzureFileRetriever : AzureStorageConnectorBase, IFileRetriever
    {
        public async Task<byte[]> GetFile(IRepositoryFile file, string containerName)
        {
            CloudBlobContainer container = await this.GetContainer(containerName, this.ConnectionStringName).ConfigureAwait(false);

            CloudBlockBlob blockBlob = container.GetBlockBlobReference(file.FileName);

            MemoryStream stream = new MemoryStream();
            await blockBlob.DownloadToStreamAsync(stream).ConfigureAwait(false);

            return stream.ToArray();
        }
    }
}