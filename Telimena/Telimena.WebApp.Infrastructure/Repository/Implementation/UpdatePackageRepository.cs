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
        public UpdatePackageRepository(DbContext dbContext)
        {
            this.TelimenaContext = dbContext as TelimenaContext;
        }

        protected TelimenaContext TelimenaContext;

        #region Implementation of IUpdatePackageRepository
        public Task<UpdatePackage> StorePackage(int programId, string version, Stream fileStream, string fileName)
        {
            if (!Version.TryParse(version, out Version _))
            {
                throw new InvalidOperationException("Version string not valid");
            }
            var pkg = new UpdatePackage(programId, version);

            this.TelimenaContext.UpdatePackages.Add(pkg);
            return Task.FromResult(pkg);

        }

        public Task<UpdatePackage> GetPackage(int packageId)
        {
            throw new NotImplementedException();
        }

        public Task<List<UpdatePackage>> GetAllPackages(int programId)
        {
            return this.TelimenaContext.UpdatePackages.Where(x => x.ProgramId == programId).ToListAsync();
        }
        #endregion
    }
}