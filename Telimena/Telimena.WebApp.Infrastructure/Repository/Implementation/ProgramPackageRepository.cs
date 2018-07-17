namespace Telimena.WebApp.Infrastructure.Repository.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.Models;
    using Database;

    internal class ProgramPackageRepository : IProgramPackageRepository
    {
        public ProgramPackageRepository(DbContext dbContext, IFileSaver fileSaver)
        {
            this.FileSaver = fileSaver;
            this.TelimenaContext = dbContext as TelimenaContext;
        }

        protected TelimenaContext TelimenaContext { get;  }
        protected IFileSaver FileSaver { get;  }

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
                byte[] result;
                using (FileStream stream = File.Open(pkg.FileLocation, FileMode.Open))
                {
                    result = new byte[stream.Length];
                    await stream.ReadAsync(result, 0, (int)stream.Length);
                }
                return result;
            }

            return null;
        }



        public async Task<ProgramPackageInfo> GetPackageInfoForProgram(int programId)
        {
            List<ProgramPackageInfo> packages = await this.TelimenaContext.ProgramPackages.Where(x => x.ProgramId == programId).OrderByDescending(x => x.Id).ToListAsync();
            return packages.FirstOrDefault();
        }
    }
}