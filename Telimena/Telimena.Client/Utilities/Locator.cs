using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TelimenaClient
{
    internal class Locator
    {
        public Locator(LiveProgramInfo programInfo) : this(programInfo, AppDomain.CurrentDomain.BaseDirectory)
        {
        }

        internal Locator(LiveProgramInfo programInfo, string basePath)
        {
            this.programInfo = programInfo;
            this.telimenaWorkingDirectory = new Lazy<DirectoryInfo>(() =>
            {
                DirectoryInfo folderInAppData = this.CreateAppFolderInAppData();

                if (folderInAppData == null || !folderInAppData.FullName.IsDirectoryWritable())
                {
                    return new DirectoryInfo(basePath);
                }

                return folderInAppData;
            });
            this.updatesFolderName = new Lazy<string>(() =>
            {
                if (this.telimenaWorkingDirectory.Value.FullName.Contains(this.GetAppDataFolder().FullName))
                {
                    return "Updates";
                }

                return this.programInfo.Program.Name + " Updates";
            });
        }

        private readonly LiveProgramInfo programInfo;
        private readonly Lazy<string> updatesFolderName;

        /// <summary>
        ///     Gets the telimena working directory - should be the program path, but it might be not write-accessible, in which
        ///     case try working from my documents
        /// </summary>
        /// <returns>DirectoryInfo.</returns>
        private readonly Lazy<DirectoryInfo> telimenaWorkingDirectory;

        public static class Static
        {
            public static FileInfo BuildUpdatePackagePath(DirectoryInfo updatesFolder, UpdatePackageData packageData)
            {
                return new FileInfo(Path.Combine(updatesFolder.FullName, packageData.Version, packageData.FileName));
            }
        }

        public FileInfo BuildUpdatePackagePath(DirectoryInfo updatesFolder, UpdatePackageData packageData)
        {
            return Static.BuildUpdatePackagePath(updatesFolder, packageData);
        }

        public DirectoryInfo GetCurrentUpdateSubfolder(IEnumerable<UpdatePackageData> packagesToDownload)
        {
            DirectoryInfo dir = this.GetUpdatesParentFolder();
            UpdatePackageData latestPkg = packagesToDownload.OrderByDescending(x => x.Version, new TelimenaVersionStringComparer()).First();
            return new DirectoryInfo(Path.Combine(dir.FullName, latestPkg.Version));
        }

        public FileInfo GetInstallerPackagePath(FileDownloadResult result)
        {
            return new FileInfo(Path.Combine(this.GetUpdatesParentFolder().FullName, result.FileName));
        }

        public FileInfo GetUpdater()
        {
            return new FileInfo(Path.Combine(this.GetUpdatesParentFolder().FullName, this.programInfo.UpdaterName));
        }

        private DirectoryInfo GetAppDataFolder()
        {
            return new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
        }

        private DirectoryInfo CreateAppFolderInAppData()
        {
            try
            {
                DirectoryInfo dir = this.GetAppDataFolder();
                DirectoryInfo dirInfo = new DirectoryInfo(Path.Combine(dir.FullName, "Telimena", this.programInfo.Program.Name));
                dirInfo.Create();
                return dirInfo;
            }
            catch (Exception)
            {
                //well, apparently we cannot write there
            }

            return null;
        }

        private DirectoryInfo GetUpdatesParentFolder()
        {
            return new DirectoryInfo(Path.Combine(this.telimenaWorkingDirectory.Value.FullName, this.updatesFolderName.Value));
        }
    }
}