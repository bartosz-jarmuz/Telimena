using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Telimena.WebApp.Infrastructure.Repository.FileStorage
{
    public class AssemblyVersionReader : IAssemblyVersionReader
    {
        public async Task<string> GetFileVersion(Stream stream)
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            using (Stream file = File.Create(tempFilePath))
            {
                await stream.CopyToAsync(file);
            }

            var unzippedPath = this.GetUnzippedPath(tempFilePath);


            return FileVersionInfo.GetVersionInfo(unzippedPath).FileVersion;

        }

        private string GetUnzippedPath(string maybeZipPath)
        {
            try
            {
                ZipFile.ExtractToDirectory(maybeZipPath, Path.GetTempPath());
                return Path.Combine(Path.GetTempPath(), Path.GetFileName(maybeZipPath));
            }
            catch (Exception)
            {
                return maybeZipPath;
            }
        }
    }
}