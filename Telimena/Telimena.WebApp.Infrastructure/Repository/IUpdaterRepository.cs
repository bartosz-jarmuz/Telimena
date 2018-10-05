using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;
using Telimena.WebApp.Infrastructure.Repository.Implementation;

namespace Telimena.WebApp.Infrastructure.Repository
{
    public interface IUpdaterRepository 
    {
        Task<UpdaterPackageInfo> GetNewestCompatibleUpdater(Program program, string version, string toolkitVersion, bool includingBeta);

        Task<byte[]> GetPackage(int packageId, IFileRetriever fileRetriever);
        Task<UpdaterPackageInfo> StorePackageAsync(Updater updater, string minimumRequiredToolkitVersion, Stream fileStream, IFileSaver fileSaver);
        Task<Updater> GetUpdater(string updaterInternalName);
        Task<Updater> GetUpdater(int id);
        Updater Add(string fileName, string internalName, TelimenaUser user);
        UpdaterPackageInfo GetPackageForVersion(Updater updaterInternalName, string version);
        Task<UpdaterPackageInfo> GetPackageInfo(int packageId);
        Task<IEnumerable<UpdaterPackageInfo>> GetPackages(string updaterInternalName);
        Task<IEnumerable<UpdaterPackageInfo>> GetAllPackages();
    }
}