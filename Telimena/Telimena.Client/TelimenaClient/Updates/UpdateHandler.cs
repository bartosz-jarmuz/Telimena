using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TelimenaClient
{
    internal partial class UpdateHandler
    {
        public UpdateHandler(IMessenger messenger, LiveProgramInfo programInfo, IReceiveUserInput inputReceiver
            , IInstallUpdates updateInstaller)
        {
            this.messenger = messenger;
            this.programInfo = programInfo;
            this.inputReceiver = inputReceiver;
            this.updateInstaller = updateInstaller;
            this.locator = new Locator(programInfo);
        }

        private readonly IMessenger messenger;
        private readonly LiveProgramInfo programInfo;
        private readonly IReceiveUserInput inputReceiver;
        private readonly IInstallUpdates updateInstaller;
        private readonly Locator locator;

        public async Task HandleUpdates(UpdateResponse programUpdateResponse, UpdateResponse updaterUpdateResponse)
        {
            try
            {
                IReadOnlyList<UpdatePackageData> packagesToInstall = null;
                if (programUpdateResponse.UpdatePackages == null || !programUpdateResponse.UpdatePackages.Any())
                {
                    return;
                }

                packagesToInstall = programUpdateResponse.UpdatePackages;

                FileInfo updaterFile = await this.InstallUpdater(updaterUpdateResponse, this.locator.GetUpdatesParentFolder()).ConfigureAwait(false);
                //FileInfo updaterFile = await this.InstallUpdater(updaterUpdateResponse, PathFinder.GetUpdatesParentFolder(Telimena.GetTelimenaWorkingDirectory(this.programInfo), GetUpdatesFolderName(this.programInfo))).ConfigureAwait(false);
                if (packagesToInstall != null && packagesToInstall.Any())
                {
                    await this.DownloadUpdatePackages(packagesToInstall).ConfigureAwait(false);

                    FileInfo instructionsFile = UpdateInstructionCreator.CreateInstructionsFile(packagesToInstall, this.programInfo.Program);
                    string maxVersion = packagesToInstall.GetMaxVersion();
                    bool installUpdatesNow = this.inputReceiver.ShowInstallUpdatesNowQuestion(maxVersion, this.programInfo);
                    if (installUpdatesNow)
                    { 
                        this.updateInstaller.InstallUpdates(instructionsFile, updaterFile);
                    }
                }
            }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"An error occured while handling updates", ex);
                }
        }

        private string GetUpdaterVersion()
        {
            var updaterFile = this.locator.GetUpdater();
            if (updaterFile.Exists)
            {
                var version = FileVersionInfo.GetVersionInfo(updaterFile.FullName);
                return version.FileVersion;
            }
            else
            {
                return "0.0.0.0";
            }
        }

        protected async Task StoreUpdatePackage(UpdatePackageData pkgData, DirectoryInfo updatesFolder)
        {
            Stream stream = await this.messenger.DownloadFile(ApiRoutes.DownloadUpdatePackage + "?id=" + pkgData.Id).ConfigureAwait(false);
            FileInfo pkgFile = new FileInfo(Path.Combine(updatesFolder.FullName, pkgData.FileName));
            await SaveStreamToPath(pkgData, pkgFile, stream).ConfigureAwait(false);
        }

        private static async Task SaveStreamToPath(UpdatePackageData pkgData, FileInfo pkgFile, Stream stream)
        {
            FileStream fileStream = null;
            try
            {
                pkgFile.Directory?.Create();
                fileStream = new FileStream(pkgFile.FullName, FileMode.Create, FileAccess.Write, FileShare.None);
                await stream.CopyToAsync(fileStream).ContinueWith(copyTask => { fileStream.Close(); }).ConfigureAwait(false);
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
                DirectoryInfo updatesFolder = this.locator.GetCurrentUpdateSubfolder(packagesToDownload);
                Directory.CreateDirectory(updatesFolder.FullName);

                foreach (UpdatePackageData updatePackageData in packagesToDownload)
                {
                    downloadTasks.Add(this.StoreUpdatePackage(updatePackageData, updatesFolder));
                }

                await Task.WhenAll(downloadTasks).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occured while downloading update packages", ex);
            }
        }

        private async Task<FileInfo> InstallUpdater(UpdateResponse response, DirectoryInfo updatesFolder)
        {
            UpdatePackageData pkgData = response?.UpdatePackages?.FirstOrDefault();
            var updaterExecutable = this.locator.GetUpdater();
            if (pkgData == null)
            {
                return updaterExecutable;
            }

            Stream stream = await this.messenger.DownloadFile(ApiRoutes.DownloadUpdaterUpdatePackage + "?id=" + pkgData.Id).ConfigureAwait(false);
            FileInfo pkgFile = new FileInfo(Path.Combine(updatesFolder.FullName, pkgData.FileName));
            if (pkgFile.Exists)
            {
                pkgFile.Delete();
            }

            try
            {
                await SaveStreamToPath(pkgData, pkgFile, stream).ConfigureAwait(false);

            }
            catch (IOException)
            {

            }
            await this.updateInstaller.InstallUpdaterUpdate(pkgFile, updaterExecutable).ConfigureAwait(false);
            return updaterExecutable;
        }
    }
}