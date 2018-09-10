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

    internal class UpdaterRepository : Repository<UpdaterInfo>, IUpdaterRepository
    {
        private IFileSaver FileSaver { get; }
        private IFileRetriever FileRetriever { get; }

        public UpdaterRepository(DbContext dbContext, IFileSaver fileSaver, IFileRetriever fileRetriever) : base(dbContext)
        {
            this.FileSaver = fileSaver;
            this.FileRetriever = fileRetriever;
            this.TelimenaContext = dbContext as TelimenaContext;
        }

        public async Task<int> Save()
        {
             return await this.TelimenaContext.SaveChangesAsync();
        }

        protected TelimenaContext TelimenaContext { get; }

        public async Task<UpdaterInfo> StorePackageAsync(string version, Stream fileStream)
        {
            if (!Version.TryParse(version, out Version _))
            {
                throw new InvalidOperationException("Version string not valid");
            }

            var pkg = new UpdaterInfo( version, fileStream.Length);

            this.TelimenaContext.UpdaterInfo.Add(pkg);

            await this.FileSaver.SaveFile(pkg, fileStream);

            return pkg;
        }

        public async Task<byte[]> GetPackage(int packageId)
        {
            var pkg = await this.TelimenaContext.UpdaterInfo.FirstOrDefaultAsync(x => x.Id == packageId);

            if (pkg != null)
            {
                return await this.FileRetriever.GetFile(pkg);
            }

            return null;
        }

    }
}