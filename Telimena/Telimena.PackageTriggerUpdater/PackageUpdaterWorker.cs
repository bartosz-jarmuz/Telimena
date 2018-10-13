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

            public void TriggerUpdate(UpdaterStartupSettings settings, UpdateInstructions instructions)
            {
                string packagePath = instructions.PackagePaths.FirstOrDefault();

            if (packagePath != null && File.Exists(packagePath))
                {
                    var package = new FileInfo(packagePath);

                    var executable = this.GetExecutablePackage(package);

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
                    var updDirPath = packagePath.DirectoryName + " Extracted";
                    PrepareAndValidatePackage(packagePath, updDirPath);
                    var executablePackagePath = Directory.GetFiles(updDirPath).FirstOrDefault();
                    if (executablePackagePath == null)
                    {
                        Console.WriteLine($"Failed to find executable package in {updDirPath}");
                        Console.ReadKey();
                        return null;
                    }
                    else
                    {
                        return new FileInfo(executablePackagePath);
                    }
                }
                else
                {
                    return packagePath;
                }

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

                return ;
            }
        }
    }
