using AutoMapper;
using Telimena.WebApp.Core.Models;
using TelimenaClient;

namespace Telimena.WebApp.Infrastructure
{
    public class AutoMapperDomainProfile : Profile
    {
        public AutoMapperDomainProfile()
        {
            this.CreateMap<ProgramUpdatePackageInfo, UpdatePackageData>()
                .ForMember(x => x.StoredFilePath, o => o.Ignore())
                .ForMember(x => x.DownloadUrl, o => o.Ignore())
                ;

            this.CreateMap<UpdaterPackageInfo, UpdatePackageData>()
                .ForMember(x => x.StoredFilePath, o => o.Ignore())
                .ForMember(x => x.DownloadUrl, o => o.Ignore())
                ;

            this.CreateMap<TelimenaPackageInfo, UpdatePackageData>()
                .ForMember(x => x.StoredFilePath, o => o.Ignore())
                .ForMember(x => x.DownloadUrl, o => o.Ignore())
                ;

            this.CreateMap<UserInfo, ClientAppUser>()
                .ForMember(x => x.Id, o => o.Ignore())
                .ForMember(x => x.IpAddresses, o => o.Ignore())
                .ForMember(x => x.IpAddressesString, o => o.Ignore())
                .ForMember(x => x.RegisteredDate, o => o.Ignore());

            this.CreateMap<AssemblyInfo, ProgramAssembly>().ForMember(x => x.PrimaryOf, o => o.Ignore()).ForMember(x => x.ProgramId, o => o.Ignore())
                .ForMember(x => x.Id, o => o.Ignore()).ForMember(x => x.Versions, o => o.Ignore()).ForMember(x => x.LatestVersion, o => o.Ignore())
                .ForMember(x => x.Program, o => o.Ignore());

            this.CreateMap<ProgramInfo, Program>().ForMember(x => x.ProgramAssemblies, o => o.Ignore())
                .ForMember(x => x.DeveloperAccount, o => o.Ignore())
                .ForMember(x => x.RegisteredDate, o => o.Ignore())
                .ForMember(x => x.Views, o => o.Ignore())
                .ForMember(x => x.TelemetrySummaries, o => o.Ignore())
                .ForMember(x => x.Events, o => o.Ignore())
                .ForMember(x => x.Id, o => o.Ignore())
                .ForMember(x => x.Updater, o => o.Ignore())
                .ForMember(x => x.Description, o => o.Ignore());
        }
    }
}