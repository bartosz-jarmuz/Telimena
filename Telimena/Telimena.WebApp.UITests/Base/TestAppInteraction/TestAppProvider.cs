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

        public static class FileNames
        {
            public const string TestAppV1 = "TestApp v1.0.0.0.zip";
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
                var pcs = Process.GetProcessesByName("AutomaticTestsClient.exe");
                foreach (Process process in pcs)
                {
                    process.Kill();
                }
                targetDir.Delete(true);
            }
            targetDir.Create();

            ZipFile.ExtractToDirectory(compressedFile.FullName, targetDir.FullName);

            return targetDir.GetFiles().FirstOrDefault(x => x.Name.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
