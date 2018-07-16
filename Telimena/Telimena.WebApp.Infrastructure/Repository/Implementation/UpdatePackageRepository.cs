// -----------------------------------------------------------------------
//  <copyright file="FunctionRepository.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

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

    internal class UpdatePackageRepository : IUpdatePackageRepository
    {
        private IFileSaver FileSaver { get; }

        public UpdatePackageRepository(DbContext dbContext, IFileSaver fileSaver)
        {
            this.FileSaver = fileSaver;
            this.TelimenaContext = dbContext as TelimenaContext;
        }

        protected TelimenaContext TelimenaContext { get; }

        public async Task<UpdatePackage> StorePackageAsync(int programId, string version, Stream fileStream, string fileName)
        {
            if (!Version.TryParse(version, out Version _))
            {
                throw new InvalidOperationException("Version string not valid");
            }
            var pkg = new UpdatePackage(fileName, programId, version, fileStream.Length);

            this.TelimenaContext.UpdatePackages.Add(pkg);

            await this.FileSaver.SaveFile(pkg, fileStream);

            return pkg;

        }

        public Task<UpdatePackage> GetPackage(int packageId)
        {
            throw new NotImplementedException();
        }

        public Task<List<UpdatePackage>> GetAllPackages(int programId)
        {
            return this.TelimenaContext.UpdatePackages.Where(x => x.ProgramId == programId).ToListAsync();
        }
    }
}