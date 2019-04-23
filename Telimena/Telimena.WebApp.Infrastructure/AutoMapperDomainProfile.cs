using AutoMapper;
using Telimena.WebApp.Core.DTO.MappableToClient;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Portal;
using Telimena.WebApp.Core.Models.Telemetry;

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
                .ForMember(x => x.ReleaseNotes, o => o.Ignore())
                .ForMember(x => x.DownloadUrl, o => o.Ignore())
                ;

            this.CreateMap<TelimenaPackageInfo, UpdatePackageData>()
                .ForMember(x => x.StoredFilePath, o => o.Ignore())
                .ForMember(x => x.ReleaseNotes, o => o.Ignore())
                .ForMember(x => x.DownloadUrl, o => o.Ignore())
                ;

            this.CreateMap<UserInfo, ClientAppUser>()
                .ForMember(x => x.Id, o => o.Ignore())
                .ForMember(x => x.PublicId, o => o.Ignore())
                .ForMember(x => x.IpAddresses, o => o.Ignore())
                .ForMember(x => x.IpAddressesString, o => o.Ignore())
                .ForMember(x => x.FirstSeenDate, o => o.Ignore());

            this.CreateMap<AssemblyInfo, ProgramAssembly>()
                .ForMember(x => x.ProgramId, o => o.Ignore())
                .ForMember(x => x.Versions, o => o.Ignore())
                .ForMember(x => x.Program, o => o.Ignore());

            this.CreateMap<ProgramInfo, Program>()
                .ForMember(x => x.DeveloperTeam, o => o.Ignore())
                .ForMember(x => x.TelemetryKey, o => o.Ignore())
                .ForMember(x => x.RegisteredDate, o => o.Ignore())
                .ForMember(x => x.Id, o => o.Ignore())
                .ForMember(x => x.PublicId, o => o.Ignore())
                .ForMember(x => x.Updater, o => o.Ignore())
                .ForMember(x => x.Description, o => o.Ignore());
        }
    }
}