using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotNetLittleHelpers;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Database;

namespace Telimena.WebApp.Infrastructure.Repository.Implementation
{
    internal class ProgramPackageRepository : Repository<ProgramPackageInfo>, IProgramPackageRepository
    {
        public ProgramPackageRepository(DbContext dbContext, IFileSaver fileSaver, IFileRetriever fileRetriever) : base(dbContext)
        {
            this.FileSaver = fileSaver;
            this.FileRetriever = fileRetriever;
            this.TelimenaContext = dbContext as TelimenaContext;
        }

        protected TelimenaContext TelimenaContext { get; }
        protected IFileSaver FileSaver { get; }
        protected IFileRetriever FileRetriever { get; }

        public async Task<ProgramPackageInfo> StorePackageAsync(int programId, Stream fileStream, string fileName, string supportedToolkitVersion)
        {
            ObjectValidator.Validate(() => this.TelimenaContext.ToolkitPackages.Any(x => x.Version == supportedToolkitVersion)
                , new ArgumentException($"There is no toolkit package with version [{supportedToolkitVersion}]"));

            ProgramPackageInfo pkg = new ProgramPackageInfo(fileName, programId, fileStream.Length, supportedToolkitVersion);
            this.TelimenaContext.ProgramPackages.Add(pkg);

            await this.FileSaver.SaveFile(pkg, fileStream);

            return pkg;
        }

        public async Task<byte[]> GetPackage(int packageId)
        {
            ProgramPackageInfo pkg = await this.TelimenaContext.ProgramPackages.FirstOrDefaultAsync(x => x.Id == packageId);

            if (pkg != null)
            {
                return await this.FileRetriever.GetFile(pkg);
            }

            return null;
        }

        public async Task<ProgramPackageInfo> GetLatestProgramPackageInfo(int programId)
        {
            List<ProgramPackageInfo> packages =
                await this.TelimenaContext.ProgramPackages.Where(x => x.ProgramId == programId).OrderByDescending(x => x.Id).ToListAsync();
            return packages.FirstOrDefault();
        }
    }
}