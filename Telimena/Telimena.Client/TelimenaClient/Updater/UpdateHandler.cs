using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Telimena.Client
{
    internal class UpdateHandler
    {
        public UpdateHandler(IMessenger messenger, ProgramInfo programInfo, bool suppressAllErrors, IReceiveUserInput inputReceiver,
            IInstallUpdates updateInstaller)
        {
            this.Messenger = messenger;
            this.ProgramInfo = programInfo;
            this.SuppressAllErrors = suppressAllErrors;
            this.InputReceiver = inputReceiver;
            this.UpdateInstaller = updateInstaller;
        }

        private IMessenger Messenger { get; }
        private ProgramInfo ProgramInfo { get; }
        private bool SuppressAllErrors { get; }
        private IReceiveUserInput InputReceiver { get; }
        private IInstallUpdates UpdateInstaller { get; }

        protected string UpdatesFolderName => this.ProgramInfo?.Name + " Updates";

        public async Task HandleUpdates(UpdateResponse response, BetaVersionSettings betaVersionSettings)
        {
            try
            {
                IReadOnlyList<UpdatePackageData> packagesToInstall = null;
                if (response.UpdatePackagesIncludingBeta == null)
                {
                    return;
                }

                packagesToInstall = this.DeterminePackagesToInstall(response, betaVersionSettings);

                if (packagesToInstall != null && packagesToInstall.Any())
                {
                    await this.DownloadUpdatePackages(packagesToInstall);

                    FileInfo instructions = UpdateInstructionCreator.CreateInstructionsFile(packagesToInstall);

                    bool installUpdatesNow = this.InputReceiver.ShowInstallUpdatesNowQuestion(packagesToInstall);

                    if (installUpdatesNow)
                    {
                        this.UpdateInstaller.InstallUpdates(instructions);
                    }
                }
            }
            catch (Exception)
            {
                if (!this.SuppressAllErrors)
                {
                    throw;
                }
            }
        }

        private IReadOnlyList<UpdatePackageData> DeterminePackagesToInstall(UpdateResponse response, BetaVersionSettings betaVersionSettings)
        {
            IReadOnlyList<UpdatePackageData> packagesToInstall = null;
            if (betaVersionSettings == BetaVersionSettings.UseBeta)
            {
                packagesToInstall = response.UpdatePackagesIncludingBeta;
            }
            else if (betaVersionSettings == BetaVersionSettings.IgnoreBeta)
            {
                packagesToInstall = response.UpdatePackages;
            }
            else if (betaVersionSettings == BetaVersionSettings.AskUserEachTime && response.UpdatePackagesIncludingBeta.Any(x => x.IsBeta))
            {
                bool includeBetaPackages = this.InputReceiver.ShowIncludeBetaPackagesQuestion(response);
                if (includeBetaPackages)
                {
                    packagesToInstall = response.UpdatePackagesIncludingBeta;
                }
                else
                {
                    packagesToInstall = response.UpdatePackages;
                }
            }

            return packagesToInstall;
        }


        private DirectoryInfo GetUpdatesFolder(IEnumerable<UpdatePackageData> packagesToDownload, string basePath)
        {
            string path = Path.Combine(basePath, this.UpdatesFolderName);
            UpdatePackageData latestPkg = packagesToDownload.OrderBy(x => x.Version, new VersionStringComparer()).First();
            return new DirectoryInfo(Path.Combine(path, latestPkg.Version));
        }


        public async Task DownloadUpdatePackages(IReadOnlyList<UpdatePackageData> packagesToDownload)
        {
            try
            {
                List<Task> downloadTasks = new List<Task>();
                DirectoryInfo updatesFolder = this.GetUpdatesFolder(packagesToDownload, AppDomain.CurrentDomain.BaseDirectory);
                Directory.CreateDirectory(updatesFolder.FullName);

                foreach (UpdatePackageData updatePackageData in packagesToDownload)
                {
                    downloadTasks.Add(this.StoreUpdatePackage(updatePackageData, updatesFolder));
                }

                await Task.WhenAll(downloadTasks);
            }
            catch (Exception)
            {
                if (!this.SuppressAllErrors)
                {
                    throw;
                }
            }
        }

        protected async Task StoreUpdatePackage(UpdatePackageData pkgData, DirectoryInfo updatesFolder)
        {
            Stream stream = await this.Messenger.DownloadFile(ApiRoutes.DownloadUpdatePackage + "?id=" + pkgData.Id);
            string pkgFilePath = Path.Combine(updatesFolder.FullName, pkgData.FileName);
            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(pkgFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
                await stream.CopyToAsync(fileStream).ContinueWith(
                    copyTask => { fileStream.Close(); });
                pkgData.StoredFilePath = pkgFilePath;
            }
            catch
            {
                fileStream?.Close();

                throw;
            }
        }
    }
}