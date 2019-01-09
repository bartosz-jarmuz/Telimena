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
            FileDownloadResult result = await this.messenger.DownloadFile(pkgData.DownloadUrl).ConfigureAwait(false);
            FileInfo pkgFile = this.locator.BuildUpdatePackagePath(updatesFolder, pkgData);
            Trace.WriteLine($"{result.FileName} to be stored into {pkgFile.FullName}");
            
            // this.DeleteFileIfExists(pkgFile);

            await SaveStreamToPath(pkgData, pkgFile, result.Stream).ConfigureAwait(false);
        }

        private void DeleteFileIfExists(FileInfo pkgFile)
        {
            try
            {
                pkgFile.Delete();
            }
            catch (Exception) { //
                                }
        }

        private static async Task SaveStreamToPath(UpdatePackageData pkgData, FileInfo pkgFile, Stream stream)
        {
            FileStream fileStream = null;
            try
            {
                pkgFile.Directory?.Create();
                using (fileStream = new FileStream(pkgFile.FullName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    await stream.CopyToAsync(fileStream).ContinueWith(copyTask => {
                        fileStream.Close();
                        stream.Close(); }
                    ).ConfigureAwait(false);
                    pkgData.StoredFilePath = pkgFile.FullName;
                }
            }
            catch (Exception ex)
            {
                fileStream?.Close();
                using (fileStream = new FileStream(pkgFile.FullName + 2, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    await stream.CopyToAsync(fileStream).ContinueWith(copyTask => {
                        fileStream.Close();
                        stream.Close();
                    }
                    ).ConfigureAwait(false);
                    pkgData.StoredFilePath = pkgFile.FullName;
                }
                throw new IOException("Error while saving stream to file", ex);
            }
        }

        private async Task DownloadUpdatePackages(IReadOnlyList<UpdatePackageData> packagesToDownload)
        {
            try
            {
                List<Task> downloadTasks = new List<Task>();
                DirectoryInfo currentUpdateSubfolder = this.locator.GetCurrentUpdateSubfolder(packagesToDownload);
                Directory.CreateDirectory(currentUpdateSubfolder.FullName);
                foreach (UpdatePackageData updatePackageData in packagesToDownload)
                {
                    Trace.WriteLine($"Tr: {updatePackageData.DownloadUrl}" + updatePackageData.DownloadUrl);

                    downloadTasks.Add(this.StoreUpdatePackage(updatePackageData, currentUpdateSubfolder));
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
            var updaterExecutable = this.locator.GetUpdater(this.programInfo);
            if (pkgData == null)
            {
                return updaterExecutable;
            }

            FileDownloadResult result = await this.messenger.DownloadFile(pkgData.DownloadUrl).ConfigureAwait(false);
            FileInfo pkgFile = this.locator.GetInstallerPackagePath(result);
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