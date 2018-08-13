namespace Telimena.WebApp.Infrastructure.Repository.Implementation
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.Models;
    using Database;

    internal class ProgramPackageRepository : IProgramPackageRepository
    {
        public ProgramPackageRepository(DbContext dbContext, IFileSaver fileSaver, IFileRetriever fileRetriever)
        {
            this.FileSaver = fileSaver;
            this.FileRetriever = fileRetriever;
            this.TelimenaContext = dbContext as TelimenaContext;
        }

        protected TelimenaContext TelimenaContext { get;  }
        protected IFileSaver FileSaver { get;  }
        protected IFileRetriever FileRetriever { get; }

        public async Task<ProgramPackageInfo> StorePackageAsync(int programId, Stream fileStream, string fileName)
        {
            ProgramPackageInfo pkg = new ProgramPackageInfo(fileName, programId, fileStream.Length);
            this.TelimenaContext.ProgramPackages.Add(pkg);

            await this.FileSaver.SaveFile(pkg, fileStream);

            return pkg;
        }

        public async Task<byte[]> GetPackage(int packageId)
        {
            var pkg = await this.TelimenaContext.ProgramPackages.FirstOrDefaultAsync(x => x.Id == packageId);

            if (pkg != null)
            {
                return await this.FileRetriever.GetFile(pkg);
            }

            return null;
        }



        public async Task<ProgramPackageInfo> GetLatestProgramPackageInfo(int programId)
        {
            List<ProgramPackageInfo> packages = await this.TelimenaContext.ProgramPackages.Where(x => x.ProgramId == programId).OrderByDescending(x => x.Id).ToListAsync();
            return packages.FirstOrDefault();
        }
    }
}