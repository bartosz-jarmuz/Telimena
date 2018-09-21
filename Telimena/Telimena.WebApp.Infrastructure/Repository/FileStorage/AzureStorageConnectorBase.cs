﻿using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Telimena.WebApp.Infrastructure.Repository.FileStorage
{
    public abstract class AzureStorageConnectorBase
    {
        protected readonly string ConnectionStringName = "FileRepositoryConnection";
        protected virtual string ContentType { get; } = "application/zip";
        protected async Task<CloudBlobContainer> GetContainer(string containerName, string connectionStringName)
        {
            string storageConnection = CloudConfigurationManager.GetSetting(this.ConnectionStringName);
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnection);

            CloudBlobClient client = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = client.GetContainerReference(containerName);

            await container.CreateIfNotExistsAsync();
            return container;
        }
    }
}