using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace Telimena.Updater
{
    internal static class FileReplacer
    {
        public static void ReplaceFiles(DirectoryInfo rootProgramFolder, FileInfo package, DirectoryInfo extractedPackageFolder)
        {
            FileInfo[] updatedFiles = extractedPackageFolder.GetFiles("*", SearchOption.AllDirectories);
            foreach (FileInfo updatedFile in updatedFiles)
            {
                FileInfo[] correspondingFiles = rootProgramFolder.GetFiles(updatedFile.Name, SearchOption.AllDirectories);
                correspondingFiles = correspondingFiles.Where(x => !x.DirectoryName.StartsWith(extractedPackageFolder.Parent.Parent.FullName)).ToArray();
                if (correspondingFiles.Any())
                {
                    foreach (FileInfo correspondingFile in correspondingFiles)
                    {
                        TryCopy(updatedFile, correspondingFile);
                    }
                }
                else
                {
                    CopyNewFile(updatedFile, rootProgramFolder, extractedPackageFolder);
                }
            }
        }

        private static void CopyNewFile(FileInfo updatedFile, DirectoryInfo rootFolder, DirectoryInfo rootUpdatesFolder)
        {
            string relativePathPart = updatedFile.FullName.Replace(rootUpdatesFolder.FullName, "");
            FileInfo targetFile = new FileInfo(rootFolder.FullName + relativePathPart);

            TryCopy(updatedFile, targetFile);
        }

        private static void TryCopy(FileInfo source, FileInfo destination)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    Directory.CreateDirectory(destination.DirectoryName);
                    File.Copy(source.FullName, destination.FullName, true);
                    return;
                }
                catch (Exception)
                {
                    Thread.Sleep(150 * (i + 1));
                    //catch any error several times, then don't catch anymore
                }
            }

            File.Copy(source.FullName, destination.FullName, true);
        }
    }
}