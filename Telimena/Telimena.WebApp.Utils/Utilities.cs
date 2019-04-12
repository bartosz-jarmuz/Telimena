using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetLittleHelpers;

namespace Telimena.WebApp.Infrastructure
{
    public static class Utilities
    {
        
        /// <summary>
        /// Ensures the stream is zipped - or zips it if needed
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="fileStream">The file stream.</param>
        /// <returns>Task&lt;Stream&gt;.</returns>
        public static async Task<Stream> ZipTheStreamIfNeeded(string fileName, Stream fileStream)
        {
            if (fileName.EndsWith(".msi", StringComparison.InvariantCultureIgnoreCase))
            {
                return fileStream;
            }

            if (ZipHelpers.IsZipCompressedData(fileStream))
            {
                return fileStream;
            }
            else
            {
                string originalFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), Path.GetFileNameWithoutExtension(fileName), fileName);
                DirectoryInfo directoryToCompress = Directory.CreateDirectory(Path.GetDirectoryName(originalFilePath));
                using (Stream compressedStream = File.Create(originalFilePath))
                {
                    await fileStream.CopyToAsync(compressedStream).ConfigureAwait(false);
                }

                var zippedFilePath = directoryToCompress.FullName + ".zip";
                ZipFile.CreateFromDirectory(directoryToCompress.FullName, zippedFilePath);

                return File.OpenRead(zippedFilePath);
            }
        }

    }
}
