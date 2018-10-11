using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TelimenaClient
{
    internal partial class UpdateHandler
    {
        public static class PathFinder
        {
            public static FileInfo GetUpdaterExecutable(DirectoryInfo workingDirectory, string updatesFolderName, string updaterFileName)
            {
                DirectoryInfo updatesDir = GetUpdatesParentFolder(workingDirectory, updatesFolderName);
                return new FileInfo(Path.Combine(updatesDir.FullName, updaterFileName));
            }

            public static DirectoryInfo GetUpdatesParentFolder(DirectoryInfo workingDirectory, string updatesFolderName)
            {
                string path = Path.Combine(workingDirectory.FullName, updatesFolderName);
                return new DirectoryInfo(path);
            }

            public static DirectoryInfo GetUpdatesSubfolder(DirectoryInfo workingDirectory, string updatesFolderName, IEnumerable<UpdatePackageData> packagesToDownload)
            {
                DirectoryInfo dir = GetUpdatesParentFolder(workingDirectory, updatesFolderName);
                UpdatePackageData latestPkg = packagesToDownload.OrderByDescending(x => x.Version, new TelimenaVersionStringComparer()).First();
                return new DirectoryInfo(Path.Combine(dir.FullName, latestPkg.Version));
            }
        }
    }
}