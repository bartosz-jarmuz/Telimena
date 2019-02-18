using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using DotNetLittleHelpers;
using Telimena.WebApp.Core;
using Telimena.WebApp.Core.DTO.MappableToClient;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;

namespace Telimena.WebApp.Infrastructure.Repository.Implementation
{
    #region Using

    #endregion

    internal class UpdatePackageRepository : Repository<ProgramUpdatePackageInfo>, IUpdatePackageRepository
    {
        private readonly IAssemblyStreamVersionReader streamVersionReader;

        public UpdatePackageRepository(DbContext dbContext, IAssemblyStreamVersionReader streamVersionReader) : base(dbContext)
        {
            this.streamVersionReader = streamVersionReader;
            this.TelimenaContext = dbContext as TelimenaContext;
        }

        private readonly string containerName = "update-packages";

        protected TelimenaContext TelimenaContext { get; }

        public async Task<ProgramUpdatePackageInfo> StorePackageAsync(Program program, string packageName, Stream fileStream, string supportedToolkitVersion, bool isBeta, string releaseNotes, IFileSaver fileSaver)
        {
            string actualVersion = await this.streamVersionReader.GetVersionFromPackage(program.PrimaryAssembly.GetFileName(), fileStream,  true).ConfigureAwait(false);
            fileStream.Position = 0;

            string actualToolkitVersion = await this.streamVersionReader.GetVersionFromPackage(DefaultToolkitNames.TelimenaAssemblyName, fileStream, false).ConfigureAwait(false);
            fileStream.Position = 0;
            fileStream = await Utilities.EnsureStreamIsZipped(DefaultToolkitNames.TelimenaAssemblyName, fileStream).ConfigureAwait(false);

            if (actualToolkitVersion != null)
            {
                supportedToolkitVersion = actualToolkitVersion;
            }

            ObjectValidator.Validate(() => Version.TryParse(supportedToolkitVersion, out Version _)
                , new InvalidOperationException($"[{supportedToolkitVersion}] is not a valid version string"));
            ObjectValidator.Validate(() => this.TelimenaContext.ToolkitPackages.Any(x => x.Version == supportedToolkitVersion)
                , new ArgumentException($"There is no toolkit package with version [{supportedToolkitVersion}]"));

            ProgramUpdatePackageInfo pkg = await this.TelimenaContext.UpdatePackages.Where(x => x.ProgramId == program.Id
                                                                                                && x.Version == actualVersion
#pragma warning disable 618
                                                                                                && x.SupportedToolkitVersion == supportedToolkitVersion).OrderByDescending(x => x.Id).FirstOrDefaultAsync().ConfigureAwait(false);
#pragma warning restore 618
            if (pkg == null)
            {
                pkg = new ProgramUpdatePackageInfo(packageName, program.Id, actualVersion, fileStream.Length, supportedToolkitVersion);
                this.TelimenaContext.UpdatePackages.Add(pkg);
            }

            pkg.UpdateContentAndMetadata(isBeta, releaseNotes);
         
            await fileSaver.SaveFile(pkg, fileStream, this.containerName).ConfigureAwait(false);

            return pkg;
        }

        public async Task<byte[]> GetPackage(Guid packageId, IFileRetriever fileRetriever)
        {
            ProgramUpdatePackageInfo pkg = await this.GetUpdatePackageInfo(packageId).ConfigureAwait(false);

            if (pkg != null)
            {
                return await fileRetriever.GetFile(pkg, this.containerName).ConfigureAwait(false);
            }

            return null;
        }

        public Task<List<ProgramUpdatePackageInfo>> GetAllPackages(int programId)
        {
            return this.TelimenaContext.UpdatePackages.Where(x => x.ProgramId == programId).ToListAsync();
        }

        public async Task<List<ProgramUpdatePackageInfo>> GetAllPackagesNewerThan( VersionData versionData, int programId)
        {
            var program = await this.TelimenaContext.Programs.SingleOrDefaultAsync(x => x.Id == programId).ConfigureAwait(false);
            List<ProgramUpdatePackageInfo> packages = await this.TelimenaContext.UpdatePackages.Where(x => x.ProgramId == programId).ToListAsync().ConfigureAwait(false);

            var currentVersion = program.DetermineProgramVersion(versionData);
#pragma warning disable 618
            var newerOnes = packages.Where(x => Utils.VersionComparison.Extensions.IsNewerVersionThan(x.Version, currentVersion)).OrderByDescending(x => x.Version, new VersionStringComparer()).ThenByDescending(x => x.Id);
#pragma warning restore 618
            List<ProgramUpdatePackageInfo> uniquePackages = this.GetUniquePackages(newerOnes);
            return uniquePackages;
        }

        private List<ProgramUpdatePackageInfo> GetUniquePackages(IOrderedEnumerable<ProgramUpdatePackageInfo> newerOnes)
        {
            var uniquePackages = new List<ProgramUpdatePackageInfo>();
            foreach (var package in newerOnes)
            {
                if (uniquePackages.All(x => x.Version != package.Version))
                {
                    uniquePackages.Add(package);
                }
            }
            return uniquePackages;
        }

        public Task<ProgramUpdatePackageInfo> GetUpdatePackageInfo(Guid id)
        {
            return this.TelimenaContext.UpdatePackages.FirstOrDefaultAsync(x => x.Guid == id);
        }
    }
}