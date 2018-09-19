using System.IO;
using System.Threading.Tasks;

namespace Telimena.Client
{
    internal interface IInstallUpdates
    {
        void InstallUpdates(FileInfo instructionsFile, FileInfo updaterFile);
        Task InstallUpdaterUpdate(FileInfo updaterPackage, FileInfo targetPath);
    }
}