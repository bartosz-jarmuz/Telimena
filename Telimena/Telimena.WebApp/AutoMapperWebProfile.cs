using AutoMapper;
using Telimena.WebApp.Core.Models;
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
        }
    }
}