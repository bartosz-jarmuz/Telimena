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

        public async Task<UpdaterPackageInfo> StorePackageAsync(string version, Stream fileStream, IFileSaver fileSaver)
        {
            if (!Version.TryParse(version, out Version _))
            {
                throw new InvalidOperationException("Version string not valid");
            }

            var pkg = new UpdaterPackageInfo( version, fileStream.Length);

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