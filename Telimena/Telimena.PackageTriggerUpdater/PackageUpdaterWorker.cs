using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using Telimena.PackageTriggerUpdater.CommandLineArguments;

namespace Telimena.PackageTriggerUpdater
{
    public class PackageUpdaterWorker
    {
        private readonly string[] nonExecutableExtensions = {".dll", ".txt", ".xml", ".docx"};

        public void TriggerUpdate(UpdaterStartupSettings settings, UpdateInstructions instructions)
        {
            string packagePath = this.GetProperPackagePath(instructions);

            if (packagePath != null && File.Exists(packagePath))
            {
                FileInfo package = new FileInfo(packagePath);

                FileInfo executable = this.GetExecutablePackage(package);

                Console.WriteLine($"Launching package {executable.FullName}.");
                Process.Start(executable.FullName);
            }
            else
            {
                Console.WriteLine($"Failed to find package from {settings.InstructionsFile}");
                Console.ReadKey();
            }
        }

        internal FileInfo GetExecutablePackage(FileInfo packagePath)
        {
            if (packagePath.FullName.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase))
            {
                string updDirPath = packagePath.DirectoryName + " Extracted";
                PrepareAndValidatePackage(packagePath, updDirPath);
                string executablePackagePath = Directory.GetFiles(updDirPath).FirstOrDefault(x =>
                    !this.nonExecutableExtensions.Contains(Path.GetExtension(x), StringComparer.InvariantCultureIgnoreCase));
                if (executablePackagePath == null)
                {
                    Console.WriteLine($"Failed to find executable package in {updDirPath}");
                    Console.ReadKey();
                    return null;
                }

                return new FileInfo(executablePackagePath);
            }

            return packagePath;
        }

        internal string GetProperPackagePath(UpdateInstructions instructions)
        {
            string packagePath = instructions.Packages.OrderByDescending(x=>x.Version, new TelimenaVersionStringComparer()).FirstOrDefault()?.Path;
            return packagePath;
        }

        private static void PrepareAndValidatePackage(FileInfo package, string updDirPath)
        {
            for (int i = 0; i < 4; i++)
            {
                if (Directory.Exists(updDirPath))
                {
                    Directory.Delete(updDirPath, true);
                }

                try
                {
                    ZipFile.ExtractToDirectory(package.FullName, updDirPath);
                    return;
                }
                catch (Exception ex)
                {
                    Thread.Sleep(500);

                    if (i >= 4)
                    {
                        Console.WriteLine(
                            "Error occurred while unpacking update package. Exception:\n" + ex.Message + "\nUpdate package will be removed and re-downloaded."
                            , "Update problem");
                        try
                        {
                            File.Delete(package.FullName);
                        }
                        catch (Exception ex2)
                        {
                            Console.WriteLine("Error occurred while deleting update package. Exception:\n" + ex2.Message);
                            Console.ReadKey();
                        }
                    }
                }
            }
        }
    }
}