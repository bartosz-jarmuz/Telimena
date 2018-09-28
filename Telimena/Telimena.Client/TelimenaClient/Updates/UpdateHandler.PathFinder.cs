using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TelimenaClient
{
    internal partial class UpdateHandler
    {
        public static class PathFinder
        {
            public static FileInfo GetUpdaterExecutable(string basePath, string updatesFolderName)
            {
                DirectoryInfo updatesDir = GetUpdatesParentFolder(basePath, updatesFolderName);
                return new FileInfo(Path.Combine(updatesDir.FullName, UpdaterFileName));
            }

            public static DirectoryInfo GetUpdatesParentFolder(string basePath, string updatesFolderName)
            {
                string path = Path.Combine(basePath, updatesFolderName);
                return new DirectoryInfo(path);
            }

            public static DirectoryInfo GetUpdatesSubfolder(string basePath, string updatesFolderName, IEnumerable<UpdatePackageData> packagesToDownload)
            {
                DirectoryInfo dir = GetUpdatesParentFolder(basePath, updatesFolderName);
                UpdatePackageData latestPkg = packagesToDownload.OrderByDescending(x => x.Version, new TelimenaVersionStringComparer()).First();
                return new DirectoryInfo(Path.Combine(dir.FullName, latestPkg.Version));
            }
        }
    }
}