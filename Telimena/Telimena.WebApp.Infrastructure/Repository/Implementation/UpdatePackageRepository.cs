// -----------------------------------------------------------------------
//  <copyright file="FunctionRepository.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
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

    internal class UpdatePackageRepository : Repository<ProgramUpdatePackageInfo>, IUpdatePackageRepository
    {
        private readonly IAssemblyVersionReader versionReader;

        public UpdatePackageRepository(DbContext dbContext, IAssemblyVersionReader versionReader) : base(dbContext)
        {
            this.versionReader = versionReader;
            this.TelimenaContext = dbContext as TelimenaContext;
        }

        private readonly string containerName = "update-packages";

        protected TelimenaContext TelimenaContext { get; }

        public async Task<ProgramUpdatePackageInfo> StorePackageAsync(Program program, string packageName, Stream fileStream, string supportedToolkitVersion, IFileSaver fileSaver)
        {
            string actualVersion = await this.versionReader.GetVersionFromPackage(program.PrimaryAssembly.GetFileName(), fileStream,  true);
            fileStream.Position = 0;

            string actualToolkitVersion = await this.versionReader.GetVersionFromPackage(DefaultToolkitNames.TelimenaAssemblyName, fileStream, false);
            fileStream.Position = 0;
            fileStream = await Utilities.EnsureStreamIsZipped(DefaultToolkitNames.TelimenaAssemblyName, fileStream);

            if (actualToolkitVersion != null)
            {
                supportedToolkitVersion = actualToolkitVersion;
            }

            ObjectValidator.Validate(() => Version.TryParse(supportedToolkitVersion, out Version _)
                , new InvalidOperationException($"[{supportedToolkitVersion}] is not a valid version string"));
            ObjectValidator.Validate(() => this.TelimenaContext.ToolkitPackages.Any(x => x.Version == supportedToolkitVersion)
                , new ArgumentException($"There is no toolkit package with version [{supportedToolkitVersion}]"));

            ProgramUpdatePackageInfo pkg = new ProgramUpdatePackageInfo(packageName, program.Id, actualVersion, fileStream.Length, supportedToolkitVersion);

            this.TelimenaContext.UpdatePackages.Add(pkg);

            await fileSaver.SaveFile(pkg, fileStream, this.containerName);

            return pkg;
        }

        public async Task<byte[]> GetPackage(int packageId, IFileRetriever fileRetriever)
        {
            ProgramUpdatePackageInfo pkg = await this.GetUpdatePackageInfo(packageId);

            if (pkg != null)
            {
                return await fileRetriever.GetFile(pkg, this.containerName);
            }

            return null;
        }

        public Task<List<ProgramUpdatePackageInfo>> GetAllPackages(int programId)
        {
            return this.TelimenaContext.UpdatePackages.Where(x => x.ProgramId == programId).ToListAsync();
        }

        public async Task<List<ProgramUpdatePackageInfo>> GetAllPackagesNewerThan(string currentVersion, int programId)
        {
          
            List<ProgramUpdatePackageInfo> packages = await this.TelimenaContext.UpdatePackages.Where(x => x.ProgramId == programId).ToListAsync();
            return packages.Where(x => x.Version.IsNewerVersionThan(currentVersion)).OrderByDescending(x => x.Version, new VersionStringComparer()).ToList();
        }

        public Task<ProgramUpdatePackageInfo> GetUpdatePackageInfo(int id)
        {
            return this.TelimenaContext.UpdatePackages.FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}