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
using TelimenaClient;

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
            this.TelimenaContext = dbContext as TelimenaContext;
        }

        protected TelimenaContext TelimenaContext { get; }
        private readonly string containerName = "toolkit-packages";

        public Task<Updater> GetUpdater(Guid id)
        {
            return this.TelimenaContext.Updaters.FirstOrDefaultAsync(x => x.Guid == id);
        }

        public Updater Add(string fileName, string internalName, TelimenaUser user)
        {
           var updater = new Updater(fileName, internalName);
            this.TelimenaContext.Updaters.Add(updater);
            updater.DeveloperAccount = user.AssociatedDeveloperAccounts.FirstOrDefault(x => x.MainUserId == user.Id);
            return updater;
        }

        public UpdaterPackageInfo GetPackageForVersion(Updater updater, string version)
        {
            ObjectValidator.Validate(() => Version.TryParse(version, out _), new ArgumentException($"[{version}] is not a valid version string"));
            return updater.Packages.Where(x => x.Version == version).OrderByDescending(x => x.Id).FirstOrDefault(); ;
        }

        public Task<UpdaterPackageInfo> GetPackageInfo(Guid packageGuid)
        {
            return this.TelimenaContext.UpdaterPackages.SingleOrDefaultAsync(x => x.Guid == packageGuid);
        }

        public async Task<IEnumerable<UpdaterPackageInfo>> GetPackages(string updaterInternalName)
        {
            return (await this.TelimenaContext.UpdaterPackages.Where(x => x.Updater.InternalName == updaterInternalName).ToListAsync().ConfigureAwait(false)).AsReadOnly();
        }

        public async Task<IEnumerable<UpdaterPackageInfo>> GetAllPackages()
        {
            return (await this.TelimenaContext.UpdaterPackages.ToListAsync().ConfigureAwait(false)).AsReadOnly();
        }

        public async Task<UpdaterPackageInfo> GetNewestCompatibleUpdater(Program program, string version, string toolkitVersion, bool includingBeta)
        {
            ObjectValidator.Validate(() => Version.TryParse(version, out _), new ArgumentException($"[{version}] is not a valid version string"));
            ObjectValidator.Validate(() => Version.TryParse(toolkitVersion, out _), new ArgumentException($"[{toolkitVersion}] is not a valid version string"));

            Updater updater = await this.GetUpdaterForProgram(program).ConfigureAwait(false);

            List<UpdaterPackageInfo> newerOnes = updater.Packages.Where(x => TelimenaClient.Extensions.IsNewerVersionThan(x.Version, version))
                    .OrderByDescending(x => x.Version, new TelimenaVersionStringComparer()).ThenByDescending(x=>x.Id).ToList();
            

            if (newerOnes.Any())
            {
                List<UpdaterPackageInfo> compatibleOnes = newerOnes.Where(x => toolkitVersion.IsNewerOrEqualVersion(x.MinimumRequiredToolkitVersion))
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
            return await this.TelimenaContext.Updaters.Where(x => x.IsPublic).ToListAsync().ConfigureAwait(false);
        }

        public async Task<Updater> GetUpdater(string updaterInternalName)
        {
            var updater = await this.TelimenaContext.Updaters.FirstOrDefaultAsync(x => x.InternalName == updaterInternalName).ConfigureAwait(false);
            if (updater == null && updaterInternalName == DefaultToolkitNames.UpdaterInternalName) //create the default updater
            {
                updater = new Updater(DefaultToolkitNames.UpdaterFileName, DefaultToolkitNames.UpdaterInternalName);
                updater.DeveloperAccount = await this.TelimenaContext.Developers.SingleOrDefaultAsync(x => x.Name == DefaultToolkitNames.TelimenaSystemDevTeam).ConfigureAwait(false);
            }
            else if (updater == null && updaterInternalName == DefaultToolkitNames.PackageTriggerUpdaterInternalName) //create the default package  updater
            {
                updater = new Updater(DefaultToolkitNames.PackageTriggerUpdaterFileName, DefaultToolkitNames.PackageTriggerUpdaterInternalName);
                updater.DeveloperAccount = await this.TelimenaContext.Developers.SingleOrDefaultAsync(x => x.Name == DefaultToolkitNames.TelimenaSystemDevTeam).ConfigureAwait(false);
            }

            if (updater != null && updater.DeveloperAccount == null)
            {
                updater.DeveloperAccount = await this.TelimenaContext.Developers.SingleOrDefaultAsync(x => x.Name == DefaultToolkitNames.TelimenaSystemDevTeam).ConfigureAwait(false);
            }

            return updater;

        }


        private async Task<Updater> GetUpdaterForProgram(Program program)
        {
            var updater = program.Updater;
            if (updater == null)
            {
                updater = await this.TelimenaContext.Updaters.FirstOrDefaultAsync(x => x.InternalName == DefaultToolkitNames.UpdaterInternalName).ConfigureAwait(false);
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

            UpdaterPackageInfo pkg = await this.TelimenaContext.UpdaterPackages.Where(x =>
                x.FileName == updater.FileName && x.Version == actualVersion && x.MinimumRequiredToolkitVersion == minimumRequiredToolkitVersion &&
                x.Updater.InternalName == updater.InternalName).OrderByDescending(x => x.Id).FirstOrDefaultAsync().ConfigureAwait(false);

            if (pkg == null)
            {
                pkg = new UpdaterPackageInfo(actualVersion, updater.FileName, fileStream.Length, minimumRequiredToolkitVersion);
                this.TelimenaContext.UpdaterPackages.Add(pkg);
                pkg.Updater = updater;
            }
            pkg.UpdateWithNewContent(fileStream.Length);

            await fileSaver.SaveFile(pkg, fileStream, this.containerName).ConfigureAwait(false);

            return pkg;
        }

        

        public async Task<byte[]> GetPackage(int packageId, IFileRetriever fileRetriever)
        {
            UpdaterPackageInfo pkg = await this.TelimenaContext.UpdaterPackages.FirstOrDefaultAsync(x => x.Id == packageId).ConfigureAwait(false);

            if (pkg != null)
            {
                return await fileRetriever.GetFile(pkg, this.containerName).ConfigureAwait(false);
            }

            return null;
        }
    }
}