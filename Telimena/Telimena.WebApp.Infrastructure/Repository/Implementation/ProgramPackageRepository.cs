using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotNetLittleHelpers;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;

namespace Telimena.WebApp.Infrastructure.Repository.Implementation
{
    public class ProgramPackageRepository : Repository<ProgramPackageInfo>, IProgramPackageRepository
    {
        public ProgramPackageRepository(DbContext dbContext) : base(dbContext)
        {
            this.telimenaContext = dbContext as TelimenaContext;
        }

        private readonly TelimenaContext telimenaContext;
        private readonly string containerName = "program-packages";
        public async Task<ProgramPackageInfo> StorePackageAsync(int programId, Stream fileStream, string fileName, string supportedToolkitVersion
            , IFileSaver fileSaver)
        {
            ObjectValidator.Validate(() => this.telimenaContext.ToolkitPackages.Any(x => x.Version == supportedToolkitVersion)
                , new ArgumentException($"There is no toolkit package with version [{supportedToolkitVersion}]"));

            ProgramPackageInfo pkg = new ProgramPackageInfo(fileName, programId, fileStream.Length, supportedToolkitVersion);
            this.telimenaContext.ProgramPackages.Add(pkg);

            await fileSaver.SaveFile(pkg, fileStream, this.containerName);

            return pkg;
        }

        public async Task<byte[]> GetPackage(int packageId, IFileRetriever fileRetriever)
        {
            ProgramPackageInfo pkg = await this.telimenaContext.ProgramPackages.FirstOrDefaultAsync(x => x.Id == packageId);

            if (pkg != null)
            {
                return await fileRetriever.GetFile(pkg, this.containerName);
            }

            return null;
        }

        public async Task<ProgramPackageInfo> GetLatestProgramPackageInfo(int programId)
        {
            List<ProgramPackageInfo> packages =
                await this.telimenaContext.ProgramPackages.Where(x => x.ProgramId == programId).OrderByDescending(x => x.Id).ToListAsync();
            return packages.FirstOrDefault();
        }
    }
}