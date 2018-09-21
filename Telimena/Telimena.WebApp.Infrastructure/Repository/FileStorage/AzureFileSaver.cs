using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using Telimena.WebApp.Core.Models;

namespace Telimena.WebApp.Infrastructure.Repository.FileStorage
{
    public class AzureFileSaver : AzureStorageConnectorBase, IFileSaver
    {
        public async Task SaveFile(IRepositoryFile file, Stream fileStream, string containerName)
        {
            CloudBlobContainer container = await this.GetContainer(containerName, this.ConnectionStringName);

            CloudBlockBlob blockBlob = container.GetBlockBlobReference(file.FileName);
            blockBlob.Properties.ContentType = this.ContentType;

            await blockBlob.UploadFromStreamAsync(fileStream);

            file.FileLocation = blockBlob.Uri?.ToString();
        }
    }
}