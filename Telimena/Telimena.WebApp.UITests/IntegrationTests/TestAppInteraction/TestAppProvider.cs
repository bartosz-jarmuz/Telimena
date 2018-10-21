using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Telimena.WebApp.UITests.IntegrationTests.TestAppInteraction
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

        public static FileInfo ExtractApp(string fileName)
        {
            FileInfo zipPath = AppsFolder.GetFiles().FirstOrDefault(x=>x.Name == fileName);
            if (zipPath == null)
            {
                throw  new FileNotFoundException($"Failed to find {fileName} file in {AppsFolder.FullName}");
            }
            var dir = new DirectoryInfo(Path.Combine(zipPath.DirectoryName, zipPath.Name + "_Extracted"));
            if (dir.Exists)
            {
                FileInfo exe = dir.GetFiles().FirstOrDefault(x => x.Name.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase));
                if (exe != null && exe.Exists)
                {
                    exe.Delete();
                }
                dir.Delete(true);
            }
            ZipFile.ExtractToDirectory(zipPath.FullName, dir.FullName);

            return dir.GetFiles().FirstOrDefault(x => x.Name.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
