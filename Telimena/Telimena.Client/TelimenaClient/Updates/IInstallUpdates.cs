using System.IO;

namespace Telimena.Client
{
    internal interface IInstallUpdates
    {
        void InstallUpdates(FileInfo instructionsFile, FileInfo updaterFile);
    }
}