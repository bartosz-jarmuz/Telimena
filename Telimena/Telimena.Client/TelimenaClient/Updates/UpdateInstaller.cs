using System;
using System.Diagnostics;
using System.IO;

namespace Telimena.Client
{

    internal static class StartInfoCreator
    {
        public static ProcessStartInfo CreateStartInfo(FileInfo instructionsFile, FileInfo updaterFile)
        {
            return new ProcessStartInfo()
            {
                FileName = updaterFile.FullName,
                Arguments = $"\"instructions:{instructionsFile.FullName}\""
            };
        }
    }
    internal class UpdateInstaller : IInstallUpdates
    {
        public void InstallUpdates(FileInfo instructionsFile, FileInfo updaterFile)
        {
            this.VerifyFilesExist(instructionsFile, updaterFile);
            Process process = new Process
            {
                StartInfo = StartInfoCreator.CreateStartInfo(instructionsFile, updaterFile)
            };

            process.Start();
            Environment.Exit(0);
        }

        

        private void VerifyFilesExist(FileInfo instructionsFile, FileInfo updaterFile)
        {
            if (instructionsFile== null || !File.Exists(instructionsFile.FullName))
            {
                throw new FileNotFoundException($"Failed to find instructions file at path: {instructionsFile.FullName}", instructionsFile.FullName);
            }
            if (updaterFile == null || !File.Exists(updaterFile.FullName))
            {
                throw new FileNotFoundException($"Failed to find updater executable at path: {updaterFile.FullName}", updaterFile.FullName);
            }
        }
    }
}