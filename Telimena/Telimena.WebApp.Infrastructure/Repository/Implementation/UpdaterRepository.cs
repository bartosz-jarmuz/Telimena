// -----------------------------------------------------------------------
//  <copyright file="FunctionRepository.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

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
    #region Using

    #endregion

    internal class UpdaterRepository : Repository<UpdaterPackageInfo>, IUpdaterRepository
    {

        public UpdaterRepository(DbContext dbContext) : base(dbContext)
        {
            this.TelimenaContext = dbContext as TelimenaContext;
        }

        protected TelimenaContext TelimenaContext { get; }
        private readonly string containerName = "toolkit-packages";
        public async Task<int> Save()
        {
            return await this.TelimenaContext.SaveChangesAsync();
        }

        public async Task<UpdaterPackageInfo> GetNewestCompatibleUpdater(string version, string toolkitVersion, bool includingBeta)
        {
            ObjectValidator.Validate(() => Version.TryParse(version, out _), new ArgumentException($"[{version}] is not a valid version string"));
            ObjectValidator.Validate(() => Version.TryParse(toolkitVersion, out _), new ArgumentException($"[{toolkitVersion}] is not a valid version string"));

            UpdaterPackageInfo current = await this.TelimenaContext.UpdaterPackages.FirstOrDefaultAsync(x => x.Version == version);

            List<UpdaterPackageInfo> newerOnes;
            if (current != null)
            {
                newerOnes = (await this.TelimenaContext.UpdaterPackages.Where(x => x.Id > current.Id).ToListAsync())
                    .OrderByDescending(x => x.Version, new TelimenaVersionStringComparer()).ToList();
            }
            else
            {
                newerOnes = (await this.TelimenaContext.UpdaterPackages.ToListAsync()).Where(x => x.Version.IsNewerVersionThan(version))
                    .OrderByDescending(x => x.Version, new TelimenaVersionStringComparer()).ToList();
            }

            if (newerOnes.Any())
            {
                List<UpdaterPackageInfo> compatibleOnes = newerOnes.Where(x => toolkitVersion.IsNewerOrEqualVersion(x.MinimumRequiredToolkitVersion)).ToList();
                if (includingBeta)
                {
                    return compatibleOnes.FirstOrDefault();
                }

                return compatibleOnes.FirstOrDefault(x => !x.IsBeta);
            }

            return null;
        }

        public async Task<UpdaterPackageInfo> StorePackageAsync(string version, string minimumRequiredToolkitVersion, Stream fileStream, IFileSaver fileSaver)
        {
            ObjectValidator.Validate(() => Version.TryParse(version, out _), new ArgumentException($"[{version}] is not a valid version string"));

            UpdaterPackageInfo pkg = new UpdaterPackageInfo(version, fileStream.Length, minimumRequiredToolkitVersion);

            this.TelimenaContext.UpdaterPackages.Add(pkg);

            await fileSaver.SaveFile(pkg, fileStream, this.containerName);

            return pkg;
        }

        public async Task<byte[]> GetPackage(int packageId, IFileRetriever fileRetriever)
        {
            UpdaterPackageInfo pkg = await this.TelimenaContext.UpdaterPackages.FirstOrDefaultAsync(x => x.Id == packageId);

            if (pkg != null)
            {
                return await fileRetriever.GetFile(pkg, this.containerName);
            }

            return null;
        }
    }
}