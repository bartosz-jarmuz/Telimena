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
            , IInstallUpdates updateInstaller, Locator locator)
        {
            this.messenger = messenger;
            this.programInfo = programInfo;
            this.inputReceiver = inputReceiver;
            this.updateInstaller = updateInstaller;
            this.locator = locator;
        }

        private readonly IMessenger messenger;
        private readonly LiveProgramInfo programInfo;
        private readonly IReceiveUserInput inputReceiver;
        private readonly IInstallUpdates updateInstaller;
        private readonly Locator locator;

        public async Task HandleUpdates(IReadOnlyList<UpdatePackageData> packagesToInstall, UpdatePackageData updaterUpdate)
        {
            try
            {
                if (packagesToInstall != null && packagesToInstall.Any())
                {
                    FileInfo updaterFile = await this.InstallUpdater(updaterUpdate).ConfigureAwait(false);
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

        protected async Task StoreUpdatePackage(UpdatePackageData pkgData, DirectoryInfo updatesFolder)
        {
            FileDownloadResult result = await this.messenger.DownloadFile(ApiRoutes.DownloadUpdatePackage + "?id=" + pkgData.Id).ConfigureAwait(false);
            FileInfo pkgFile = new FileInfo(Path.Combine(updatesFolder.FullName, result.FileName));
            await SaveStreamToPath(pkgData, pkgFile, result.Stream).ConfigureAwait(false);
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

        private async Task<FileInfo> InstallUpdater(UpdatePackageData pkgData)
        {
            var updaterExecutable = this.locator.GetUpdater();
            if (pkgData == null)
            {
                return updaterExecutable;
            }

            var result = await this.messenger.DownloadFile(ApiRoutes.DownloadUpdaterUpdatePackage + "?id=" + pkgData.Id).ConfigureAwait(false);
            FileInfo pkgFile = new FileInfo(Path.Combine(this.locator.GetUpdatesParentFolder().FullName, result.FileName));
            if (pkgFile.Exists)
            {
                pkgFile.Delete();
            }

            try
            {
                await SaveStreamToPath(pkgData, pkgFile, result.Stream).ConfigureAwait(false);
            }
            catch (IOException)
            {
            }
            await this.updateInstaller.InstallUpdaterUpdate(pkgFile, updaterExecutable).ConfigureAwait(false);
            return updaterExecutable;
        }
    }
}