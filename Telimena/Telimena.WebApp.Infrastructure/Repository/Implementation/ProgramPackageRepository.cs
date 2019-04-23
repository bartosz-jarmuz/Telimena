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
            string actualVersion = await this.versionReader.GetVersionFromPackage(program.PrimaryAssembly.GetFileName(), fileStream, packageFileName, true).ConfigureAwait(false);
            fileStream.Position = 0;
            ObjectValidator.Validate(() => Version.TryParse(actualVersion, out Version _)
                , new InvalidOperationException($"[{actualVersion}] is not a valid version string"));

            string actualToolkitVersion = await this.versionReader.GetVersionFromPackage(DefaultToolkitNames.TelimenaAssemblyName, fileStream, packageFileName, false).ConfigureAwait(false);
            fileStream.Position = 0;
            fileStream = await Utilities.ZipTheStreamIfNeeded(packageFileName, fileStream).ConfigureAwait(false);

            ProgramPackageInfo pkg = await this.telimenaPortalContext.ProgramPackages.Where(x => x.ProgramId == program.Id
                                                                                                && x.Version == actualVersion
#pragma warning disable 618
                                                                                                && x.SupportedToolkitVersion == actualToolkitVersion).OrderByDescending(x => x.Id).FirstOrDefaultAsync().ConfigureAwait(false);
#pragma warning restore 618
            if (pkg == null)
            {
                pkg = new ProgramPackageInfo(packageFileName, program.Id, actualVersion, fileStream.Length, actualToolkitVersion);
                this.telimenaPortalContext.ProgramPackages.Add(pkg);
            }
            else
            {
                pkg.UploadedDate = DateTimeOffset.UtcNow;
            }

            await fileSaver.SaveFile(pkg, fileStream, this.containerName).ConfigureAwait(false);

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