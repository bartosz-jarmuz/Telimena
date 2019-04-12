using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using LessMsi.Msi;
using Telimena.WebApp.Utils.VersionComparison;

namespace Telimena.WebApp.Infrastructure.Repository.FileStorage
{
    public class AssemblyStreamVersionReader : IAssemblyStreamVersionReader
    {
        public async Task<string> GetFileVersion(Stream stream, string expectedFileName, bool expectSingleFile)
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            using (Stream file = File.Create(tempFilePath))
            {
                await stream.CopyToAsync(file).ConfigureAwait(false);
            }

            var unzippedPath = this.GetUnzippedPath(tempFilePath, expectedFileName, expectSingleFile);


            var version = TelimenaVersionReader.Read(unzippedPath, VersionTypes.FileVersion);
            if (string.IsNullOrEmpty(version))
            {
                throw new InvalidOperationException("Cannot extract version info from uploaded file");
            }

            return version;
        }

        public async Task<string> GetVersionFromPackage(string nameOfFileToCheck, Stream fileStream
            , string packageFileName, bool required = true)
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "package");
            Directory.CreateDirectory(Path.GetDirectoryName(tempFilePath));
            using (FileStream fs = new FileStream(tempFilePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fs).ConfigureAwait(false);
            }

            fileStream.Seek(0, SeekOrigin.Begin);

            string version = null;
            if (packageFileName.EndsWith(".msi", StringComparison.InvariantCultureIgnoreCase))
            {
                version = GetFromMsi(nameOfFileToCheck, tempFilePath);
            }
            else
            {
                version = GetFromZip(nameOfFileToCheck, tempFilePath);
            }
            
            if (version == null && required)
            {
                throw new FileNotFoundException( $"Failed to find the required assembly in the uploaded package. [{nameOfFileToCheck}] should be present.", nameOfFileToCheck);
            }

            return version;
        }

        private static string GetFromMsi(string nameOfFileToCheck, string zipPath)
        {
            Wixtracts.ExtractFiles(zipPath, Path.GetDirectoryName(zipPath));
            foreach (string file in Directory.GetFiles(Path.GetDirectoryName(zipPath), "*", SearchOption.AllDirectories))
            {
                if (Path.GetFileName(file).Equals(nameOfFileToCheck, StringComparison.InvariantCultureIgnoreCase))
                {
                       return TelimenaVersionReader.Read(file, VersionTypes.FileVersion);
                }
            }
            return null;
        }

        private static string GetFromZip(string nameOfFileToCheck, string zipPath)
        {
            ZipFile.ExtractToDirectory(zipPath, Path.GetDirectoryName(zipPath));
            foreach (string file in Directory.GetFiles(Path.GetDirectoryName(zipPath), "*", SearchOption.AllDirectories))
            {
                if (Path.GetFileName(file).Equals(nameOfFileToCheck, StringComparison.InvariantCultureIgnoreCase))
                {
                    {
                        return TelimenaVersionReader.Read(file, VersionTypes.FileVersion);
                    }
                }
            }

            return null;
        }

        private string GetUnzippedPath(string maybeZipPath, string expectedFileName, bool expectSingleFile)
        {
            string extractedFolderPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(extractedFolderPath);
            try
            {
                ZipFile.ExtractToDirectory(maybeZipPath, extractedFolderPath);
               
            }
            catch (Exception)
            {
                return maybeZipPath;
            }
            var path = Path.Combine(extractedFolderPath, expectedFileName);
            if (!File.Exists(path))
            {
                throw new InvalidOperationException($"The uploaded package does not contain expected file: {expectedFileName}.");
            }

            if (expectSingleFile)
            {
                if (Directory.GetFiles(extractedFolderPath).Length != 1)
                {
                    throw new InvalidOperationException($"The uploaded package contains more than one file.");
                }
            }
            return path;
        }
    }
}