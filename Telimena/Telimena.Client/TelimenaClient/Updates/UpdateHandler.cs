using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Telimena.Client
{
    internal partial class UpdateHandler
    {
        public UpdateHandler(IMessenger messenger, ProgramInfo programInfo, bool suppressAllErrors, IReceiveUserInput inputReceiver
            , IInstallUpdates updateInstaller)
        {
            this.messenger = messenger;
            this.programInfo = programInfo;
            this.suppressAllErrors = suppressAllErrors;
            this.inputReceiver = inputReceiver;
            this.updateInstaller = updateInstaller;
        }

        public const string UpdaterFileName = "Updater.exe";

        private readonly IMessenger messenger;
        private readonly ProgramInfo programInfo;
        private readonly bool suppressAllErrors;
        private readonly IReceiveUserInput inputReceiver;
        private readonly IInstallUpdates updateInstaller;

        internal static string BasePath => AppDomain.CurrentDomain.BaseDirectory;

        public async Task HandleUpdates(UpdateResponse programUpdateResponse, UpdateResponse updaterUpdateResponse)
        {
            try
            {
                IReadOnlyList<UpdatePackageData> packagesToInstall = null;
                if (updaterUpdateResponse.UpdatePackages == null || !updaterUpdateResponse.UpdatePackages.Any())
                {
                    return;
                }

                packagesToInstall = programUpdateResponse.UpdatePackages;

                await this.InstallUpdater(updaterUpdateResponse, PathFinder.GetUpdatesParentFolder(BasePath, GetUpdatesFolderName(this.programInfo)));
                if (packagesToInstall != null && packagesToInstall.Any())
                {
                    await this.DownloadUpdatePackages(packagesToInstall);

                    FileInfo instructionsFile = UpdateInstructionCreator.CreateInstructionsFile(packagesToInstall, this.programInfo);

                    bool installUpdatesNow = this.inputReceiver.ShowInstallUpdatesNowQuestion(packagesToInstall);
                    FileInfo updaterFile = PathFinder.GetUpdaterExecutable(BasePath, GetUpdatesFolderName(this.programInfo));
                    if (installUpdatesNow)
                    {
                        this.updateInstaller.InstallUpdates(instructionsFile, updaterFile);
                    }
                }
            }
            catch (Exception)
            {
                if (!this.suppressAllErrors)
                {
                    throw;
                }
            }
        }

        protected internal static string GetUpdatesFolderName(ProgramInfo programInfo)
        {
            return programInfo.Name + " Updates";
        }

        protected async Task StoreUpdatePackage(UpdatePackageData pkgData, DirectoryInfo updatesFolder)
        {
            Stream stream = await this.messenger.DownloadFile(ApiRoutes.DownloadUpdatePackage + "?id=" + pkgData.Id);
            FileInfo pkgFile = new FileInfo(Path.Combine(updatesFolder.FullName, pkgData.FileName));
            await SaveStreamToPath(pkgData, pkgFile, stream);
        }

        private static async Task SaveStreamToPath(UpdatePackageData pkgData, FileInfo pkgFile, Stream stream)
        {
            FileStream fileStream = null;
            try
            {
                pkgFile.Directory?.Create();
                fileStream = new FileStream(pkgFile.FullName, FileMode.Create, FileAccess.Write, FileShare.None);
                await stream.CopyToAsync(fileStream).ContinueWith(copyTask => { fileStream.Close(); });
                pkgData.StoredFilePath = pkgFile.FullName;
            }
            catch
            {
                fileStream?.Close();

                throw;
            }
        }

        private async Task DownloadUpdatePackages(IReadOnlyList<UpdatePackageData> packagesToDownload)
        {
            try
            {
                List<Task> downloadTasks = new List<Task>();
                DirectoryInfo updatesFolder = this.GetUpdatesSubfolder(packagesToDownload, BasePath);
                Directory.CreateDirectory(updatesFolder.FullName);

                foreach (UpdatePackageData updatePackageData in packagesToDownload)
                {
                    downloadTasks.Add(this.StoreUpdatePackage(updatePackageData, updatesFolder));
                }

                await Task.WhenAll(downloadTasks);
            }
            catch (Exception)
            {
                if (!this.suppressAllErrors)
                {
                    throw;
                }
            }
        }

        private DirectoryInfo GetUpdatesSubfolder(IEnumerable<UpdatePackageData> packagesToDownload, string basePath)
        {
            return PathFinder.GetUpdatesSubfolder(basePath, GetUpdatesFolderName(this.programInfo), packagesToDownload);
        }

        private async Task InstallUpdater(UpdateResponse response, DirectoryInfo updatesFolder)
        {
            UpdatePackageData pkgData = response?.UpdatePackages?.FirstOrDefault();

            if (pkgData == null)
            {
                return;
            }

            Stream stream = await this.messenger.DownloadFile(ApiRoutes.DownloadUpdaterUpdatePackage + "?id=" + pkgData.Id);
            FileInfo pkgFile = new FileInfo(Path.Combine(updatesFolder.FullName, pkgData.FileName));
            if (pkgFile.Exists)
            {
                pkgFile.Delete();
            }

            await SaveStreamToPath(pkgData, pkgFile, stream);
            await this.updateInstaller.InstallUpdaterUpdate(pkgFile, PathFinder.GetUpdaterExecutable(BasePath, GetUpdatesFolderName(this.programInfo)));
        }
    }
}