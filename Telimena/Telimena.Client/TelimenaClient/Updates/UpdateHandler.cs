using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TelimenaClient
{
    internal partial class UpdateHandler
    {
        public UpdateHandler(IMessenger messenger, ProgramInfo programInfo, IReceiveUserInput inputReceiver
            , IInstallUpdates updateInstaller)
        {
            this.messenger = messenger;
            this.programInfo = programInfo;
            this.inputReceiver = inputReceiver;
            this.updateInstaller = updateInstaller;
        }

        private readonly IMessenger messenger;
        private readonly ProgramInfo programInfo;
        private readonly IReceiveUserInput inputReceiver;
        private readonly IInstallUpdates updateInstaller;


    



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

                FileInfo updaterFile = await this.InstallUpdater(updaterUpdateResponse, PathFinder.GetUpdatesParentFolder(Telimena.GetTelimenaWorkingDirectory(this.programInfo), GetUpdatesFolderName(this.programInfo))).ConfigureAwait(false);
                if (packagesToInstall != null && packagesToInstall.Any())
                {
                    await this.DownloadUpdatePackages(packagesToInstall).ConfigureAwait(false);

                    FileInfo instructionsFile = UpdateInstructionCreator.CreateInstructionsFile(packagesToInstall, this.programInfo);
                    string maxVersion = packagesToInstall.Where(x => x.FileName != Telimena.GetUpdaterFileName()).GetMaxVersion();
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

        protected internal static string GetUpdatesFolderName(ProgramInfo programInfo)
        {
            return programInfo.Name + " Updates";
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
                DirectoryInfo updatesFolder = this.GetUpdatesSubfolder(packagesToDownload, Telimena.GetTelimenaWorkingDirectory(this.programInfo));
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

        private DirectoryInfo GetUpdatesSubfolder(IEnumerable<UpdatePackageData> packagesToDownload, DirectoryInfo workingDirectory)
        {
            return PathFinder.GetUpdatesSubfolder(workingDirectory, GetUpdatesFolderName(this.programInfo), packagesToDownload);
        }

        private async Task<FileInfo> InstallUpdater(UpdateResponse response, DirectoryInfo updatesFolder)
        {
            UpdatePackageData pkgData = response?.UpdatePackages?.FirstOrDefault();
            var updaterExecutable = PathFinder.GetUpdaterExecutable(Telimena.GetTelimenaWorkingDirectory(this.programInfo), GetUpdatesFolderName(this.programInfo), Telimena.GetUpdaterFileName());
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