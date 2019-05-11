using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotNetLittleHelpers;
using Telimena.WebApp.Core.DTO.MappableToClient;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Portal;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;

namespace Telimena.WebApp.Infrastructure.Repository.Implementation
{
    public class ProgramPackageRepository : Repository<ProgramPackageInfo>, IProgramPackageRepository
    {
        private readonly IAssemblyStreamVersionReader versionReader;

        public ProgramPackageRepository(DbContext dbContext, IAssemblyStreamVersionReader versionReader) : base(dbContext)
        {
            this.versionReader = versionReader;
            this.telimenaPortalContext = dbContext as TelimenaPortalContext;
        }

        private readonly TelimenaPortalContext telimenaPortalContext;
        private readonly string containerName = "program-packages";

        public async Task<ProgramPackageInfo> StorePackageAsync(Program program, Stream fileStream, string packageFileName, IFileSaver fileSaver)
        {
         
            var versions = await
                this.versionReader.GetVersionsFromStream(packageFileName, fileStream
                    , program.PrimaryAssembly.GetFileName()).ConfigureAwait(false);


            fileStream = await Utilities.ZipTheStreamIfNeeded(packageFileName, fileStream).ConfigureAwait(false);

            ProgramPackageInfo pkg = await this.telimenaPortalContext.ProgramPackages.Where(x => x.ProgramId == program.Id
                                                                                                && x.Version == versions.appVersion
                                                                                                && x.SupportedToolkitVersion == versions.toolkitVersion).OrderByDescending(x => x.Id).FirstOrDefaultAsync().ConfigureAwait(false);
            if (pkg == null)
            {
                pkg = new ProgramPackageInfo(packageFileName, program.Id, versions.appVersion, fileStream.Length, versions.toolkitVersion);
                this.telimenaPortalContext.ProgramPackages.Add(pkg);
            }
            else
            {
                pkg.UploadedDate = DateTimeOffset.UtcNow;
            }

            await fileSaver.SaveFile(pkg, fileStream, this.containerName, program.TelemetryKey).ConfigureAwait(false);

            return pkg;
        }

        public async Task<byte[]> GetPackage(int packageId, IFileRetriever fileRetriever)
        {
            ProgramPackageInfo pkg = await this.telimenaPortalContext.ProgramPackages.FirstOrDefaultAsync(x => x.Id == packageId).ConfigureAwait(false);

            if (pkg != null)
            {
                return await fileRetriever.GetFile(pkg, this.containerName).ConfigureAwait(false);
            }

            return null;
        }

        public async Task<ProgramPackageInfo> GetLatestProgramPackageInfo(int programId)
        {
            List<ProgramPackageInfo> packages =
                await this.telimenaPortalContext.ProgramPackages.Where(x => x.ProgramId == programId).OrderByDescending(x => x.UploadedDate).ToListAsync().ConfigureAwait(false);
            return packages.FirstOrDefault();
        }
    }
}