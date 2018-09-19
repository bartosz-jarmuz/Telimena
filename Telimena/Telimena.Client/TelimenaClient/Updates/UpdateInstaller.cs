using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace Telimena.Client
{
    internal class UpdateInstaller : IInstallUpdates
    {

        public async Task InstallUpdaterUpdate(FileInfo updaterPackage, FileInfo targetPath)
        {
            for (int i = 0; i < 4; i++)
            {
                try
                {
                    ZipFile.ExtractToDirectory(updaterPackage.FullName, targetPath.DirectoryName);
                    break;
                }
                catch (Exception ex)
                {
                    await Task.Delay(150 * (i + 1));
                    if (i == 4)
                    {
                        throw new InvalidOperationException("Error occurred while extracting the updater package", ex);
                    }

                }
            }
            
        }

        public void InstallUpdates(FileInfo instructionsFile, FileInfo updaterFile)
        {
            this.VerifyFilesExist(instructionsFile, updaterFile);
            Process process = new Process {StartInfo = StartInfoCreator.CreateStartInfo(instructionsFile, updaterFile)};

            process.Start();
            Environment.Exit(0);
        }

        private void VerifyFilesExist(FileInfo instructionsFile, FileInfo updaterFile)
        {
            if (instructionsFile == null || !File.Exists(instructionsFile.FullName))
            {
                throw new FileNotFoundException($"Failed to find instructions file at path: {instructionsFile?.FullName}", instructionsFile?.FullName);
            }

            if (updaterFile == null || !File.Exists(updaterFile.FullName))
            {
                throw new FileNotFoundException($"Failed to find updater executable at path: {updaterFile?.FullName}", updaterFile?.FullName);
            }
        }
    }
}