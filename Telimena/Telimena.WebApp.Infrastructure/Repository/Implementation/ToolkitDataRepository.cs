using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using DotNetLittleHelpers;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;

namespace Telimena.WebApp.Infrastructure.Repository.Implementation
{
    internal sealed class ToolkitDataRepository : Repository<TelimenaToolkitData>, IToolkitDataRepository
    {
        private readonly string containerName = "toolkit-packages";
        private readonly IAssemblyVersionReader versionReader;

        public ToolkitDataRepository(DbContext dbContext, IAssemblyVersionReader versionReader) : base(dbContext)
        {
            this.versionReader = versionReader;
            this.TelimenaContext = dbContext as TelimenaContext;
        }

        private TelimenaContext TelimenaContext { get; }

        public async Task<TelimenaToolkitData> StorePackageAsync(string version, bool isBeta, bool introducesBreakingChanges, Stream fileStream, IFileSaver fileSaver)
        {
            if (!Version.TryParse(version, out Version _))
            {
                throw new InvalidOperationException("Version string not valid");
            }

            string actualVersion = await this.versionReader.GetFileVersion(fileStream);
            fileStream.Position = 0;
            fileStream = await this.EnsureStreamIsZipped(TelimenaPackageInfo.TelimenaAssemblyName, fileStream);

            TelimenaToolkitData data = await this.TelimenaContext.TelimenaToolkitData.Where(x => x.Version == actualVersion).Include(nameof(TelimenaToolkitData.TelimenaPackageInfo)).FirstOrDefaultAsync();
            if (data == null)
            {
                data = new TelimenaToolkitData(actualVersion);
                this.TelimenaContext.TelimenaToolkitData.Add(data);
            }

            if (data.TelimenaPackageInfo == null)
            {
                TelimenaPackageInfo pkg = new TelimenaPackageInfo(actualVersion, fileStream.Length);
                data.TelimenaPackageInfo = pkg;
            }

            data.TelimenaPackageInfo.UpdateWithNewContent(isBeta, introducesBreakingChanges, actualVersion, fileStream.Length);
            await fileSaver.SaveFile(data.TelimenaPackageInfo, fileStream, this.containerName);

            return data;
        }

        /// <summary>
        /// Ensures the stream is zipped - or zips it if needed
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="fileStream">The file stream.</param>
        /// <returns>Task&lt;Stream&gt;.</returns>
        private async Task<Stream> EnsureStreamIsZipped(string fileName, Stream fileStream)
        {
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
                    await fileStream.CopyToAsync(compressedStream);
                }

                var zippedFilePath = directoryToCompress.FullName + ".zip";
                ZipFile.CreateFromDirectory(directoryToCompress.FullName, zippedFilePath);

                return File.OpenRead(zippedFilePath);
            }
        }

        public async Task<byte[]> GetPackage(int toolkitDataId, IFileRetriever fileRetriever)
        {
            TelimenaToolkitData toolkitData = await this.TelimenaContext.TelimenaToolkitData.FirstOrDefaultAsync(x => x.Id == toolkitDataId);

            TelimenaPackageInfo pkg = toolkitData?.TelimenaPackageInfo;
            if (pkg != null)
            {
                return await fileRetriever.GetFile(pkg, this.containerName);
            }

            return null;
        }

        public async Task<List<TelimenaPackageInfo>> GetPackagesNewerThan(string version)
        {
            ObjectValidator.Validate(() => Version.TryParse(version, out _), new ArgumentException($"[{version}] is not a valid version string"));

            TelimenaPackageInfo current = await this.TelimenaContext.ToolkitPackages.FirstOrDefaultAsync(x => x.Version == version);

            if (current != null)
            {
                return (await this.TelimenaContext.ToolkitPackages.Where(x => x.Id > current.Id).ToListAsync())
                    .OrderByDescending(x => x.Version, new TelimenaVersionStringComparer()).ToList();
            }

            return (await this.TelimenaContext.ToolkitPackages.ToListAsync()).Where(x => x.Version.IsNewerVersionThan(version))
                .OrderByDescending(x => x.Version, new TelimenaVersionStringComparer()).ToList();
        }
    }
}