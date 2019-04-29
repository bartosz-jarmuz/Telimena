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
       
        private static DirectoryInfo AppsFolder
        {
            get
            {
                var solutionDir = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory)); //Debug
                solutionDir = solutionDir.Parent; //bin
                solutionDir = solutionDir.Parent; //project
                solutionDir = solutionDir.Parent; //solution
                return new DirectoryInfo(Path.Combine(solutionDir.FullName, "Telimena.WebApp.AppIntegrationTests", "Apps"));
            }
        }

        public static FileInfo GetFile(string fileName)
        {
            FileInfo compressedFile = AppsFolder.GetFiles().FirstOrDefault(x => x.Name == fileName);
            if (compressedFile == null)
            {
                throw new FileNotFoundException($"Failed to find {fileName} file in {AppsFolder.FullName}");
            }

            return compressedFile;
        }



        public static FileInfo ExtractApp(string fileName, string testSubfolderName, Action<string> log)
        {
            var compressedFile = GetFile(fileName);
            var targetDir = new DirectoryInfo(Path.Combine(compressedFile.DirectoryName, testSubfolderName, compressedFile.Name + "_Extracted"));
            log($"Extracting file {compressedFile} to {targetDir.FullName}");

            try
            {
                TestHelpers.DeleteDirectory(targetDir.FullName);

            }
            catch (UnauthorizedAccessException)
            {
                KillTestApps();

                TestHelpers.DeleteDirectory(targetDir.FullName);

            }
            targetDir.Create();

            ZipFile.ExtractToDirectory(compressedFile.FullName, targetDir.FullName);
            log($"FINISHED Extracting file {compressedFile} to {targetDir.FullName}");

            return targetDir.GetFiles().FirstOrDefault(x => x.Name.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase));
        }

        public static void KillTestApps()
        {
            KillProcessesByName("AutomaticTestsClient.exe");
            KillProcessesByName("AutomaticTestsClient");
            KillProcessesByName("PackageTriggerUpdaterTestApp");
            KillProcessesByName("PackageTriggerUpdaterTestApp.exe");
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
