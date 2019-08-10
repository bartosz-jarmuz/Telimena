using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using Telimena.WebApp.Core.Models.Portal;

namespace Telimena.WebApp.Infrastructure.Repository.FileStorage
{
    public class AzureFileRemover : AzureStorageConnectorBase, IFileRemover
    {
        public async Task DeleteFile(IRepositoryFile file,  string containerName)
        {
            CloudBlobContainer container = await this.GetContainer(containerName, this.ConnectionStringName).ConfigureAwait(false);
            string location = file.FileLocation.Substring(file.FileLocation.IndexOf(containerName, StringComparison.Ordinal) + containerName.Length).Trim('/');

            CloudBlockBlob blockBlob = container.GetBlockBlobReference(location);
            await blockBlob.DeleteAsync().ConfigureAwait(false);
        }
    }
}