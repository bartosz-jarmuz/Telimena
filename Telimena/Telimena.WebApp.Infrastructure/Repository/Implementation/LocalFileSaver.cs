namespace Telimena.WebApp.Infrastructure.Repository.Implementation
{
    using System.IO;
    using System.Threading.Tasks;
    using System.Web.Hosting;
    using Core.Models;

    public class LocalFileSaver : IFileSaver
    {
        public LocalFileSaver()
        {
            this.RootFolder = HostingEnvironment.MapPath("~/App_Data/");

        }

        private string RootFolder { get; }

        public async Task SaveFile(IRepositoryFile repositoryFile, Stream fileStream)
        {
            var fileLocation = Path.Combine(this.RootFolder, "FileRepo", repositoryFile.FileName);
            Directory.CreateDirectory(Path.GetDirectoryName(fileLocation));
            using (Stream file = File.Create(fileLocation))
            {
                await fileStream.CopyToAsync(file);
            }

            repositoryFile.FileLocation = fileLocation;
        }
    }
}