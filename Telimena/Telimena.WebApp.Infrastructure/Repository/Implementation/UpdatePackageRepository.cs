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

namespace Telimena.WebApp.Infrastructure.Repository.Implementation
{
    #region Using

    #endregion

    internal class UpdatePackageRepository : Repository<ProgramUpdatePackageInfo>, IUpdatePackageRepository
    {
        public UpdatePackageRepository(DbContext dbContext, IFileSaver fileSaver, IFileRetriever fileRetriever) : base(dbContext)
        {
            this.FileSaver = fileSaver;
            this.FileRetriever = fileRetriever;
            this.TelimenaContext = dbContext as TelimenaContext;
        }

        protected TelimenaContext TelimenaContext { get; }
        private IFileSaver FileSaver { get; }
        private IFileRetriever FileRetriever { get; }

        public async Task<ProgramUpdatePackageInfo> StorePackageAsync(Program program, string version, Stream fileStream, string supportedToolkitVersion)
        {
            ObjectValidator.Validate(() => Version.TryParse(version, out Version _)
                , new InvalidOperationException($"[{version}] is not a valid version string"));
            ObjectValidator.Validate(() => Version.TryParse(supportedToolkitVersion, out Version _)
                , new InvalidOperationException($"[{supportedToolkitVersion}] is not a valid version string"));
            ObjectValidator.Validate(() => this.TelimenaContext.ToolkitPackages.Any(x => x.Version == supportedToolkitVersion)
                , new ArgumentException($"There is no toolkit package with version [{supportedToolkitVersion}]"));


            string fileName = program.Name + " Update v. " + version + ".zip";
            ProgramUpdatePackageInfo pkg = new ProgramUpdatePackageInfo(fileName, program.Id, version, fileStream.Length, supportedToolkitVersion);

            this.TelimenaContext.UpdatePackages.Add(pkg);

            await this.FileSaver.SaveFile(pkg, fileStream);

            return pkg;
        }

        public async Task<byte[]> GetPackage(int packageId)
        {
            ProgramUpdatePackageInfo pkg = await this.GetUpdatePackageInfo(packageId);

            if (pkg != null)
            {
                return await this.FileRetriever.GetFile(pkg);
            }

            return null;
        }

        public Task<List<ProgramUpdatePackageInfo>> GetAllPackages(int programId)
        {
            return this.TelimenaContext.UpdatePackages.Where(x => x.ProgramId == programId).ToListAsync();
        }

        public async Task<List<ProgramUpdatePackageInfo>> GetAllPackagesNewerThan(string currentVersion, int programId)
        {
            ProgramUpdatePackageInfo currentVersionPackage = await this.TelimenaContext.UpdatePackages.FirstOrDefaultAsync(x => x.Version == currentVersion);

            if (currentVersionPackage != null)
            {
                return (await this.TelimenaContext.UpdatePackages.Where(x => x.ProgramId == programId && x.Id > currentVersionPackage.Id).ToListAsync())
                    .OrderByDescending(x => x.Version, new VersionStringComparer()).ToList();
            }

            List<ProgramUpdatePackageInfo> packages = await this.TelimenaContext.UpdatePackages.Where(x => x.ProgramId == programId).ToListAsync();
            return packages.Where(x => x.Version.IsNewerVersionThan(currentVersion)).OrderByDescending(x => x.Version, new VersionStringComparer()).ToList();
        }

        public Task<ProgramUpdatePackageInfo> GetUpdatePackageInfo(int id)
        {
            return this.TelimenaContext.UpdatePackages.FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}