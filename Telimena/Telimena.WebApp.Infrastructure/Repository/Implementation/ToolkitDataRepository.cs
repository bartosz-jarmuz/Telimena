﻿using System;
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
using TelimenaClient;

namespace Telimena.WebApp.Infrastructure.Repository.Implementation
{
    internal sealed class ToolkitDataRepository : Repository<TelimenaToolkitData>, IToolkitDataRepository
    {
        private readonly string containerName = "toolkit-packages";
        private readonly IAssemblyStreamVersionReader versionReader;

        public ToolkitDataRepository(DbContext dbContext, IAssemblyStreamVersionReader versionReader) : base(dbContext)
        {
            this.versionReader = versionReader;
            this.TelimenaContext = dbContext as TelimenaContext;
        }

        private TelimenaContext TelimenaContext { get; }

        public async Task<TelimenaToolkitData> StorePackageAsync(bool isBeta, bool introducesBreakingChanges, Stream fileStream, IFileSaver fileSaver)
        {
            string actualVersion = await this.versionReader.GetFileVersion(fileStream, DefaultToolkitNames.TelimenaAssemblyName, true).ConfigureAwait(false);
            fileStream.Position = 0;
            fileStream = await Utilities.EnsureStreamIsZipped(DefaultToolkitNames.TelimenaAssemblyName, fileStream).ConfigureAwait(false);

            TelimenaToolkitData data = await this.TelimenaContext.TelimenaToolkitData.Where(x => x.Version == actualVersion).Include(nameof(TelimenaToolkitData.TelimenaPackageInfo)).FirstOrDefaultAsync().ConfigureAwait(false);
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
            await fileSaver.SaveFile(data.TelimenaPackageInfo, fileStream, this.containerName).ConfigureAwait(false);

            return data;
        }

     

        public async Task<byte[]> GetPackage(int toolkitDataId, IFileRetriever fileRetriever)
        {
            TelimenaToolkitData toolkitData = await this.TelimenaContext.TelimenaToolkitData.FirstOrDefaultAsync(x => x.Id == toolkitDataId).ConfigureAwait(false);

            TelimenaPackageInfo pkg = toolkitData?.TelimenaPackageInfo;
            if (pkg != null)
            {
                return await fileRetriever.GetFile(pkg, this.containerName).ConfigureAwait(false);
            }

            return null;
        }

        public async Task<List<TelimenaPackageInfo>> GetPackagesNewerThan(string version)
        {
            ObjectValidator.Validate(() => Version.TryParse(version, out _), new ArgumentException($"[{version}] is not a valid version string"));

            TelimenaPackageInfo current = await this.TelimenaContext.ToolkitPackages.Where(x => x.Version == version).OrderByDescending(x => x.Id).FirstOrDefaultAsync().ConfigureAwait(false) ;
            List<TelimenaPackageInfo> packages;
            if (current != null)
            {
                packages = (await this.TelimenaContext.ToolkitPackages.Where(x => x.Id > current.Id).ToListAsync().ConfigureAwait(false))
                    .OrderByDescending(x => x.Version, new TelimenaVersionStringComparer()).ThenByDescending(x=>x.Id).ToList();
            }
            else
            {
                packages = (await this.TelimenaContext.ToolkitPackages.ToListAsync().ConfigureAwait(false)).Where(x => TelimenaClient.Extensions.IsNewerVersionThan(x.Version, version))
                .OrderByDescending(x => x.Version, new TelimenaVersionStringComparer()).ThenByDescending(x => x.Id).ToList();
            }

            return this.GetUniquePackages(packages);
        }

        private List<TelimenaPackageInfo> GetUniquePackages(IEnumerable<TelimenaPackageInfo> newerOnes)
        {
            var uniquePackages = new List<TelimenaPackageInfo>();
            foreach (var package in newerOnes)
            {
                if (uniquePackages.All(x => x.Version != package.Version))
                {
                    uniquePackages.Add(package);
                }
            }
            return uniquePackages;
        }
    }
}