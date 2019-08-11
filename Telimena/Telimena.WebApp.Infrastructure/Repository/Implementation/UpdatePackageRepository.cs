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
using Telimena.WebApp.Core.Models.Portal;
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
            this.TelimenaPortalContext = dbContext as TelimenaPortalContext;
        }

        private readonly string containerName = "update-packages";

        protected TelimenaPortalContext TelimenaPortalContext { get; }

        public async Task<ProgramUpdatePackageInfo> StorePackageAsync(Program program, string packageName, Stream fileStream, string supportedToolkitVersion, bool isBeta, string releaseNotes, IFileSaver fileSaver)
        {
            var versions = await 
                this.streamVersionReader.GetVersionsFromStream(packageName, fileStream
                    , program.PrimaryAssembly.GetFileName()).ConfigureAwait(false);



            fileStream = await Utilities.ZipTheStreamIfNeeded(packageName, fileStream).ConfigureAwait(false);

            if (versions.toolkitVersion!= null)
            {
                supportedToolkitVersion = versions.toolkitVersion;
            }

            ObjectValidator.Validate(() => Version.TryParse(supportedToolkitVersion, out Version _)
                , new InvalidOperationException($"[{supportedToolkitVersion}] is not a valid version string"));
            ObjectValidator.Validate(() => this.TelimenaPortalContext.ToolkitPackages.Any(x => x.Version == supportedToolkitVersion)
                , new ArgumentException($"There is no toolkit package with version [{supportedToolkitVersion}]"));

            ProgramUpdatePackageInfo pkg = await this.TelimenaPortalContext.UpdatePackages.Where(x => x.ProgramId == program.Id
                                                                                                && x.Version == versions.appVersion
#pragma warning disable 618
                                                                                                && x.SupportedToolkitVersion == supportedToolkitVersion).OrderByDescending(x => x.Id).FirstOrDefaultAsync().ConfigureAwait(false);
#pragma warning restore 618
            if (pkg == null)
            {
                pkg = new ProgramUpdatePackageInfo(packageName, program.Id, versions.appVersion, fileStream.Length, supportedToolkitVersion);
                this.TelimenaPortalContext.UpdatePackages.Add(pkg);
            }

            pkg.UpdateContentAndMetadata(isBeta, releaseNotes);
         
            await fileSaver.SaveFile(pkg, fileStream, this.containerName, program.TelemetryKey).ConfigureAwait(false);

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

        public async Task DeletePackage(ProgramUpdatePackageInfo pkg, IFileRemover fileRemover)
        {
            if (pkg != null)
            {
                try
                {
                    await fileRemover.DeleteFile(pkg, this.containerName).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("(404) Not Found"))
                    {
                        throw;
                    }
                }

                this.TelimenaPortalContext.UpdatePackages.Remove(pkg);
            }

        }

        public Task<List<ProgramUpdatePackageInfo>> GetAllPackages(int programId)
        {
            return this.TelimenaPortalContext.UpdatePackages.Where(x => x.ProgramId == programId).ToListAsync();
        }

        public async Task<ProgramUpdatePackageInfo> GetLatestPackage(int programId)
        {
            return (await this.TelimenaPortalContext.UpdatePackages.Where(x => x.ProgramId == programId).ToListAsync().ConfigureAwait(false)).OrderByDescending(x => x.Version, new VersionStringComparer()).ThenByDescending(x => x.Id).FirstOrDefault();
        }

        public async Task<List<ProgramUpdatePackageInfo>> GetAllPackagesNewerThan( VersionData versionData, int programId)
        {
            var program = await this.TelimenaPortalContext.Programs.SingleOrDefaultAsync(x => x.Id == programId).ConfigureAwait(false);
            List<ProgramUpdatePackageInfo> packages = await this.TelimenaPortalContext.UpdatePackages.Where(x => x.ProgramId == programId).ToListAsync().ConfigureAwait(false);

            var currentVersion = program.DetermineProgramVersion(versionData);
#pragma warning disable 618
            var newerOnes = packages.Where(x => Utils.VersionComparison.Extensions.IsNewerVersionThan(x.Version, currentVersion))
                .OrderByDescending(x => x.Version, new VersionStringComparer()).ThenByDescending(x => x.Id);
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

        public Task<ProgramUpdatePackageInfo> GetUpdatePackageInfo(int id)
        {
            return this.TelimenaPortalContext.UpdatePackages.FirstOrDefaultAsync(x => x.Id == id);
        }
        public Task<ProgramUpdatePackageInfo> GetUpdatePackageInfo(Guid id)
        {
            return this.TelimenaPortalContext.UpdatePackages.FirstOrDefaultAsync(x => x.PublicId == id);
        }
    }
}