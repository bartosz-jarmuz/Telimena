using AutoMapper;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Models.PortalUsers;

namespace Telimena.WebApp
{
    public class AutoMapperWebProfile : Profile
    {
        public AutoMapperWebProfile()
        {
            this.CreateMap<TelimenaUser, TelimenaUserViewModel>().ForMember(x => x.RoleNames, o => o.Ignore())
                .ForMember(x => x.DeveloperAccountsLed, o => o.Ignore());
        }
    }
}