using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telimena.WebApp.Infrastructure.Repository
{
    using System.IO;
    using Core.Models;

    public interface IUpdatePackageRepository
    {
        Task<UpdatePackage> StorePackageAsync(int programId, string version, Stream fileStream, string fileName);

        Task<UpdatePackage> GetPackage(int packageId);
        Task<List<UpdatePackage>> GetAllPackages(int programId);
    }
}
