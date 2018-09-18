using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotNetLittleHelpers;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Database;

namespace Telimena.WebApp.Infrastructure.Repository.Implementation
{
    internal sealed class ToolkitDataRepository : Repository<TelimenaToolkitData>, IToolkitDataRepository
    {
        public ToolkitDataRepository(DbContext dbContext) : base(dbContext)
        {
            this.TelimenaContext = dbContext as TelimenaContext;
        }

        private TelimenaContext TelimenaContext { get; }

        public async Task<TelimenaToolkitData> StorePackageAsync(string version, bool isBeta, bool introducesBreakingChanges, Stream fileStream, IFileSaver fileSaver)
        {
            if (!Version.TryParse(version, out Version _))
            {
                throw new InvalidOperationException("Version string not valid");
            }

            TelimenaToolkitData data = await this.TelimenaContext.TelimenaToolkitData.FirstOrDefaultAsync(x => x.Version == version);
            if (data == null)
            {
                data = new TelimenaToolkitData(version);
                this.TelimenaContext.TelimenaToolkitData.Add(data);
            }

            TelimenaPackageInfo pkg = new TelimenaPackageInfo(version, fileStream.Length);
            pkg.IsBeta = isBeta;
            pkg.IntroducesBreakingChanges = introducesBreakingChanges;
            data.TelimenaPackageInfo = pkg;

            await fileSaver.SaveFile(pkg, fileStream);

            return data;
        }

        public async Task<byte[]> GetPackage(int toolkitDataId, IFileRetriever fileRetriever)
        {
            TelimenaToolkitData toolkitData = await this.TelimenaContext.TelimenaToolkitData.FirstOrDefaultAsync(x => x.Id == toolkitDataId);

            TelimenaPackageInfo pkg = toolkitData?.TelimenaPackageInfo;
            if (pkg != null)
            {
                return await fileRetriever.GetFile(pkg);
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