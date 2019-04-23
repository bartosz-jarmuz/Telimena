using AutoMapper;
using Telimena.Portal.Api.Models;
using Telimena.Portal.Api.Models.DTO;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Portal;
using Telimena.WebApp.Models.PortalUsers;

namespace Telimena.WebApp
{
    /// <summary>
    /// Class AutoMapperWebProfile.
    /// </summary>
    /// <seealso cref="AutoMapper.Profile" />
    public class AutoMapperWebProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutoMapperWebProfile"/> class.
        /// </summary>
        public AutoMapperWebProfile()
        {
            this.CreateMap<TelimenaUser, TelimenaUserViewModel>().ForMember(x => x.RoleNames, o => o.Ignore())
                .ForMember(x => x.DeveloperAccountsLed, o => o.Ignore());

            this.CreateMap<UpdaterPackageInfo, UpdaterPackageInfoDto>()
                .ForMember(x => x.Id, o => o.MapFrom(y => y.PublicId));

            this.CreateMap<Updater, UpdaterDto>()
                .ForMember(x => x.Id, o => o.MapFrom(y => y.PublicId))
                .ForMember(x => x.DeveloperTeamName, o => o.MapFrom(y => y.DeveloperTeam.Name??""));

            this.CreateMap<TelimenaToolkitData, TelimenaToolkitDataDto>()
                .ForMember(x => x.Id, o => o.MapFrom(y => y.PublicId));

            this.CreateMap<TelimenaPackageInfo, TelimenaPackageInfoDto>()
                .ForMember(x => x.Id, o => o.MapFrom(y => y.PublicId));

            this.CreateMap<ProgramUpdatePackageInfo, ProgramUpdatePackageInfoDto>()
                .ForMember(x => x.Id, o => o.MapFrom(y => y.PublicId));

            this.CreateMap<ProgramPackageInfo, ProgramPackageInfoDto>()
                .ForMember(x => x.Id, o => o.MapFrom(y => y.PublicId));

        }
    }
}