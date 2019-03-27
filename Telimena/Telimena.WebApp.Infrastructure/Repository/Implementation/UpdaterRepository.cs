using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotNetLittleHelpers;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.DTO.MappableToClient;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Portal;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;
using Telimena.WebApp.Utils.VersionComparison;

namespace Telimena.WebApp.Infrastructure.Repository.Implementation
{
    #region Using

    #endregion

    internal class UpdaterRepository : IUpdaterRepository
    {
        private readonly IAssemblyStreamVersionReader streamVersionReader;

        public UpdaterRepository(DbContext dbContext, IAssemblyStreamVersionReader streamVersionReader) 
        {
            this.streamVersionReader = streamVersionReader;
            this.TelimenaPortalContext = dbContext as TelimenaPortalContext;
        }

        protected TelimenaPortalContext TelimenaPortalContext { get; }
        private readonly string containerName = "toolkit-packages";

        public Task<Updater> GetUpdater(int id)
        {
            return this.TelimenaPortalContext.Updaters.FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<Updater> GetUpdater(Guid publicId)
        {
            return this.TelimenaPortalContext.Updaters.FirstOrDefaultAsync(x => x.PublicId == publicId);
        }

        public Updater Add(string fileName, string internalName, TelimenaUser user)
        {
           var updater = new Updater(fileName, internalName);
            this.TelimenaPortalContext.Updaters.Add(updater);
            updater.DeveloperTeam = user.AssociatedDeveloperAccounts.FirstOrDefault(x => x.MainUserId == user.Id);
            return updater;
        }

        public UpdaterPackageInfo GetPackageForVersion(Updater updater, string version)
        {
            ObjectValidator.Validate(() => Version.TryParse(version, out _), new ArgumentException($"[{version}] is not a valid version string"));
            return updater.Packages.Where(x => x.Version == version).OrderByDescending(x => x.Id).FirstOrDefault(); ;
        }

        public Task<UpdaterPackageInfo> GetPackageInfo(Guid packageGuid)
        {
            return this.TelimenaPortalContext.UpdaterPackages.SingleOrDefaultAsync(x => x.PublicId == packageGuid);
        }

        public async Task<IEnumerable<UpdaterPackageInfo>> GetPackages(string updaterInternalName)
        {
            return (await this.TelimenaPortalContext.UpdaterPackages.Where(x => x.Updater.InternalName == updaterInternalName).ToListAsync().ConfigureAwait(false)).AsReadOnly();
        }

        public async Task<IEnumerable<UpdaterPackageInfo>> GetAllPackages()
        {
            return (await this.TelimenaPortalContext.UpdaterPackages.ToListAsync().ConfigureAwait(false)).AsReadOnly();
        }

        public async Task<UpdaterPackageInfo> GetNewestCompatibleUpdater(Program program, string version, string toolkitVersion, bool includingBeta)
        {
            ObjectValidator.Validate(() => Version.TryParse(version, out _), new ArgumentException($"[{version}] is not a valid version string"));
            ObjectValidator.Validate(() => Version.TryParse(toolkitVersion, out _), new ArgumentException($"[{toolkitVersion}] is not a valid version string"));

            Updater updater = await this.GetUpdaterForProgram(program).ConfigureAwait(false);

            List<UpdaterPackageInfo> newerOnes = updater.Packages.Where(x => Utils.VersionComparison.Extensions.IsNewerVersionThan(x.Version, version))
                    .OrderByDescending(x => x.Version, new TelimenaVersionStringComparer()).ThenByDescending(x=>x.Id).ToList();
            

            if (newerOnes.Any())
            {
                List<UpdaterPackageInfo> compatibleOnes = newerOnes.Where(x => string.IsNullOrEmpty(x.MinimumRequiredToolkitVersion) || toolkitVersion.IsNewerOrEqualVersion(x.MinimumRequiredToolkitVersion))
                    .OrderByDescending(x => x.Version, new TelimenaVersionStringComparer()).ThenByDescending(x => x.Id).ToList();
                if (includingBeta)
                {
                    return compatibleOnes.FirstOrDefault();
                }

                return compatibleOnes.FirstOrDefault(x => !x.IsBeta);
            }

            return null;
        }

        public async Task<IEnumerable<Updater>> GetPublicUpdaters()
        {
            return await this.TelimenaPortalContext.Updaters.Where(x => x.IsPublic).ToListAsync().ConfigureAwait(false);
        }

        public async Task<Updater> GetUpdater(string updaterInternalName)
        {
            var updater = await this.TelimenaPortalContext.Updaters.FirstOrDefaultAsync(x => x.InternalName == updaterInternalName).ConfigureAwait(false);
            if (updater == null && updaterInternalName == DefaultToolkitNames.UpdaterInternalName) //create the default updater
            {
                updater = new Updater(DefaultToolkitNames.UpdaterFileName, DefaultToolkitNames.UpdaterInternalName);
                updater.Description = DefaultUpdaterDescriptions.UpdaterDescription;
                updater.DeveloperTeam = await this.TelimenaPortalContext.Developers.SingleOrDefaultAsync(x => x.Name == DefaultToolkitNames.TelimenaSystemDevTeam).ConfigureAwait(false);
            }
            else if (updater == null && updaterInternalName == DefaultToolkitNames.PackageTriggerUpdaterInternalName) //create the default package  updater
            {
                updater = new Updater(DefaultToolkitNames.PackageTriggerUpdaterFileName, DefaultToolkitNames.PackageTriggerUpdaterInternalName);
                updater.Description = DefaultUpdaterDescriptions.PackageTriggerUpdaterDescription;
                updater.DeveloperTeam = await this.TelimenaPortalContext.Developers.SingleOrDefaultAsync(x => x.Name == DefaultToolkitNames.TelimenaSystemDevTeam).ConfigureAwait(false);
            }

            if (updater != null && updater.DeveloperTeam == null)
            {
                updater.DeveloperTeam = await this.TelimenaPortalContext.Developers.SingleOrDefaultAsync(x => x.Name == DefaultToolkitNames.TelimenaSystemDevTeam).ConfigureAwait(false);
            }

            if (updater != null && updater.Description == null)
            {
                if (updater.InternalName == DefaultToolkitNames.UpdaterInternalName)
                {
                    updater.Description = DefaultUpdaterDescriptions.UpdaterDescription;
                }
                else if (updater.InternalName == DefaultToolkitNames.PackageTriggerUpdaterInternalName)
                {
                    updater.Description = DefaultUpdaterDescriptions.PackageTriggerUpdaterDescription;
                }
            }

            return updater;

        }


        private async Task<Updater> GetUpdaterForProgram(Program program)
        {
            var updater = program.Updater;
            if (updater == null)
            {
                updater = await this.TelimenaPortalContext.Updaters.FirstOrDefaultAsync(x => x.InternalName == DefaultToolkitNames.UpdaterInternalName).ConfigureAwait(false);
                if (updater == null)
                {
                    throw new InvalidOperationException("Default updater not found!");
                }

                program.Updater = updater;

            }

            return updater;
        }

        public async Task<UpdaterPackageInfo> StorePackageAsync(Updater updater, string minimumRequiredToolkitVersion, Stream fileStream, IFileSaver fileSaver)
        {
            string actualVersion = await this.streamVersionReader.GetFileVersion(fileStream, updater.FileName, true).ConfigureAwait(false);
            fileStream.Position = 0;
            fileStream = await Utilities.EnsureStreamIsZipped(updater.FileName, fileStream).ConfigureAwait(false);

            if (string.IsNullOrEmpty(minimumRequiredToolkitVersion))
            {
                minimumRequiredToolkitVersion = "0.0.0.0";
            }

            UpdaterPackageInfo pkg = await this.TelimenaPortalContext.UpdaterPackages.Where(x =>
                x.FileName == updater.FileName && x.Version == actualVersion && x.MinimumRequiredToolkitVersion == minimumRequiredToolkitVersion &&
                x.Updater.InternalName == updater.InternalName).OrderByDescending(x => x.Id).FirstOrDefaultAsync().ConfigureAwait(false);

            if (pkg == null)
            {
                pkg = new UpdaterPackageInfo(actualVersion, updater.FileName, fileStream.Length, minimumRequiredToolkitVersion);
                this.TelimenaPortalContext.UpdaterPackages.Add(pkg);
                pkg.Updater = updater;
            }
            pkg.UpdateContentAndMetadata(fileStream.Length);

            await fileSaver.SaveFile(pkg, fileStream, this.containerName).ConfigureAwait(false);

            return pkg;
        }

        

        public async Task<byte[]> GetPackage(int packageId, IFileRetriever fileRetriever)
        {
            UpdaterPackageInfo pkg = await this.TelimenaPortalContext.UpdaterPackages.FirstOrDefaultAsync(x => x.Id == packageId).ConfigureAwait(false);

            if (pkg != null)
            {
                return await fileRetriever.GetFile(pkg, this.containerName).ConfigureAwait(false);
            }

            return null;
        }
    }
}