using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Telimena.WebApp.UITests.Base.TestAppInteraction
{
    [DeploymentItem("IntegrationTests")]
    public class TestAppProvider
    {

        public const string AutomaticTestsClientAppName = "AutomaticTestsClient";
        public const string PackageUpdaterTestAppName = "PackageTriggerUpdaterTestApp";

        public static class FileNames
        {
            public const string TestAppV1 = "TestApp v1.0.0.0.zip";
            public const string PackageUpdaterTestAppV1 = "PackageTriggerUpdaterTestApp v.1.0.0.0.zip";
        }

        private static readonly DirectoryInfo AppsFolder = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "IntegrationTests", "Apps"));

        public static FileInfo GetFile(string fileName)
        {
            FileInfo compressedFile = AppsFolder.GetFiles().FirstOrDefault(x => x.Name == fileName);
            if (compressedFile == null)
            {
                throw new FileNotFoundException($"Failed to find {fileName} file in {AppsFolder.FullName}");
            }

            return compressedFile;
        }

        public static FileInfo ExtractApp(string fileName, string testSubfolderName)
        {
            var compressedFile = GetFile(fileName);
            var targetDir = new DirectoryInfo(Path.Combine(compressedFile.DirectoryName, testSubfolderName, compressedFile.Name + "_Extracted"));
            try
            {
                if (targetDir.Exists)
                {
                    targetDir.Delete(true);
                }
            }
            catch (UnauthorizedAccessException)
            {
                KillProcessesByName("AutomaticTestsClient.exe");
                KillProcessesByName("AutomaticTestsClient");
                KillProcessesByName("PackageTriggerUpdaterTestApp");
                KillProcessesByName("PackageTriggerUpdaterTestApp.exe");
                
                targetDir.Delete(true);
            }
            targetDir.Create();

            ZipFile.ExtractToDirectory(compressedFile.FullName, targetDir.FullName);

            return targetDir.GetFiles().FirstOrDefault(x => x.Name.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase));
        }

        private static void KillProcessesByName(string name)
        {
            var pcs = Process.GetProcessesByName(name);
            foreach (Process process in pcs)
            {
                process.Kill();
            }
        }
    }
}
