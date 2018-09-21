using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.Hosting;
using Telimena.WebApp.Core.Models;

namespace Telimena.WebApp.Infrastructure.Repository.FileStorage
{
    public class LocalFileSaver : IFileSaver
    {
        public LocalFileSaver()
        {
            this.RootFolder = HostingEnvironment.MapPath("~/App_Data/");
        }

        private string RootFolder { get; }

        public async Task SaveFile(IRepositoryFile repositoryFile, Stream fileStream, string containerName)
        {
            string fileLocation = Path.Combine(this.RootFolder, "FileRepo", containerName, Guid.NewGuid().ToString(), repositoryFile.FileName);
            Directory.CreateDirectory(Path.GetDirectoryName(fileLocation));
            using (Stream file = File.Create(fileLocation))
            {
                await fileStream.CopyToAsync(file);
            }

            repositoryFile.FileLocation = fileLocation;
        }
    }
}