// -----------------------------------------------------------------------
//  <copyright file="FunctionRepository.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using DotNetLittleHelpers;

namespace Telimena.WebApp.Infrastructure.Repository.Implementation
{
    #region Using
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.Models;
    using Database;
    #endregion

    internal class UpdaterRepository : Repository<UpdaterPackageInfo>, IUpdaterRepository
    {
       

        public UpdaterRepository(DbContext dbContext) : base(dbContext)
        {
            this.TelimenaContext = dbContext as TelimenaContext;
        }

        public async Task<int> Save()
        {
             return await this.TelimenaContext.SaveChangesAsync();
        }

        protected TelimenaContext TelimenaContext { get; }

        public async Task<UpdaterPackageInfo> GetNewestCompatibleUpdater(string version, string toolkitVersion, bool includingBeta)
        {
            ObjectValidator.Validate(()=>Version.TryParse(version, out _), new ArgumentException($"[{version}] is not a valid version string"));
            ObjectValidator.Validate(()=>Version.TryParse(toolkitVersion, out _), new ArgumentException($"[{toolkitVersion}] is not a valid version string"));

            UpdaterPackageInfo current = await this.TelimenaContext.UpdaterInfo.FirstOrDefaultAsync(x => x.Version == version);

            List<UpdaterPackageInfo> newerOnes;
            if (current != null)
            {
                newerOnes = (await this.TelimenaContext.UpdaterInfo.Where(x => x.Id > current.Id).ToListAsync()).OrderByDescending(x=>x.Version, new TelimenaVersionStringComparer()).ToList();
            }
            else
            {
                newerOnes = (await this.TelimenaContext.UpdaterInfo.ToListAsync()).Where(x=>x.Version.IsNewerVersionThan(version)).OrderByDescending(x => x.Version, new TelimenaVersionStringComparer()).ToList();
            }

            if (newerOnes.Any())
            {
                List<UpdaterPackageInfo> compatibleOnes = newerOnes.Where(x => !x.MinimumRequiredToolkitVersion.IsNewerVersionThan(toolkitVersion)).ToList();
                if (includingBeta)
                {
                    return compatibleOnes.FirstOrDefault();
                }
                else
                {
                    return compatibleOnes.FirstOrDefault(x => !x.IsBeta);
                }
            }
            else
            {
                return null;
            }
        }

        public async Task<UpdaterPackageInfo> StorePackageAsync(string version, string minimumRequiredToolkitVersion, Stream fileStream, IFileSaver fileSaver)
        {
            ObjectValidator.Validate(() => Version.TryParse(version, out _), new ArgumentException($"[{version}] is not a valid version string"));

            var pkg = new UpdaterPackageInfo( version, fileStream.Length, minimumRequiredToolkitVersion);

            this.TelimenaContext.UpdaterInfo.Add(pkg);

            await fileSaver.SaveFile(pkg, fileStream);

            return pkg;
        }

        public async Task<byte[]> GetPackage(int packageId, IFileRetriever fileRetriever)
        {
            var pkg = await this.TelimenaContext.UpdaterInfo.FirstOrDefaultAsync(x => x.Id == packageId);

            if (pkg != null)
            {
                return await fileRetriever.GetFile(pkg);
            }

            return null;
        }

    }
}