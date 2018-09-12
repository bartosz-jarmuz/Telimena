using System;
using System.IO;
using Telimena.WebApp.Infrastructure.Database;

namespace Telimena.WebApp.Infrastructure.Repository.Implementation
{
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.Models;

    internal sealed class ToolkitDataRepository :  Repository<TelimenaToolkitData>, IToolkitDataRepository
    {
        private TelimenaContext TelimenaContext { get; }
        public ToolkitDataRepository(DbContext dbContext) : base(dbContext)
        {
            this.TelimenaContext = dbContext as TelimenaContext;
        }

     

        public async Task<TelimenaToolkitData> StorePackageAsync(string version, Stream fileStream, IFileSaver fileSaver)
        {
            if (!Version.TryParse(version, out Version _))
            {
                throw new InvalidOperationException("Version string not valid");
            }

            TelimenaToolkitData data = await this.TelimenaContext.TelimenaToolkitData.FirstOrDefaultAsync(x => x.Version == version); 
            if (data == null)
            {
                data = new TelimenaToolkitData(version)
                {
                };
                this.TelimenaContext.TelimenaToolkitData.Add(data);
            }

            TelimenaPackageInfo pkg = new TelimenaPackageInfo(version, fileStream.Length);
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

    }
}