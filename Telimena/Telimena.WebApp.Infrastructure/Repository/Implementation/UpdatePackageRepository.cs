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

    internal class UpdatePackageRepository : Repository<UpdatePackageInfo>, IUpdatePackageRepository
    {
        private IFileSaver FileSaver { get; }
        private IFileRetriever FileRetriever { get; }

        public UpdatePackageRepository(DbContext dbContext, IFileSaver fileSaver, IFileRetriever fileRetriever) : base(dbContext)
        {
            this.FileSaver = fileSaver;
            this.FileRetriever = fileRetriever;
            this.TelimenaContext = dbContext as TelimenaContext;
        }

        protected TelimenaContext TelimenaContext { get; }

        public async Task<UpdatePackageInfo> StorePackageAsync(int programId, string version, Stream fileStream, string fileName)
        {
            if (!Version.TryParse(version, out Version _))
            {
                throw new InvalidOperationException("Version string not valid");
            }
            var pkg = new UpdatePackageInfo(fileName, programId, version, fileStream.Length);

            this.TelimenaContext.UpdatePackages.Add(pkg);

            await this.FileSaver.SaveFile(pkg, fileStream);

            return pkg;

        }

        public async Task<byte[]> GetPackage(int packageId)
        {
            var pkg = await this.GetUpdatePackageInfo(packageId);

            if (pkg != null)
            {
                return await this.FileRetriever.GetFile(pkg);
            }

            return null;
        }

        public Task<List<UpdatePackageInfo>> GetAllPackages(int programId)
        {
            return this.TelimenaContext.UpdatePackages.Where(x => x.ProgramId == programId).ToListAsync();
        }

        public async Task<List<UpdatePackageInfo>> GetAllPackagesNewerThan(int programId, string version)
        {
            return await this.TelimenaContext.UpdatePackages.Where(x => x.ProgramId == programId && x.Version.IsNewerVersionThan(version)).OrderBy(x => x.Version, new VersionStringComparer()).ToListAsync();

        }

        public Task<UpdatePackageInfo> GetUpdatePackageInfo(int id)
        {
            return this.TelimenaContext.UpdatePackages.FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}