using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.Hosting;
using Telimena.WebApp.Core.Models;

namespace Telimena.WebApp.Infrastructure.Repository.Implementation
{
    public class LocalFileSaver : IFileSaver
    {
        public LocalFileSaver()
        {
            this.RootFolder = HostingEnvironment.MapPath("~/App_Data/");
        }

        private string RootFolder { get; }

        public async Task SaveFile(IRepositoryFile repositoryFile, Stream fileStream)
        {
            string fileLocation = Path.Combine(this.RootFolder, "FileRepo", Guid.NewGuid().ToString(), repositoryFile.FileName);
            Directory.CreateDirectory(Path.GetDirectoryName(fileLocation));
            using (Stream file = File.Create(fileLocation))
            {
                await fileStream.CopyToAsync(file);
            }

            repositoryFile.FileLocation = fileLocation;
        }
    }

    public class LocalFileRetriever : IFileRetriever
    {
        public LocalFileRetriever()
        {
            this.RootFolder = HostingEnvironment.MapPath("~/App_Data/");
        }

        private string RootFolder { get; }

        public async Task<byte[]> GetFile(IRepositoryFile repositoryFile)
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