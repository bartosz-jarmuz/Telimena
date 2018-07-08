namespace Telimena.WebApp
{
    using AutoMapper;
    using Core.Models;
    using Models.PortalUsers;

    public class AutoMapperWebProfile : Profile
    {
        public AutoMapperWebProfile()
        {
            this.CreateMap<TelimenaUser, TelimenaUserViewModel>()
                .ForMember(x => x.RoleNames, o => o.Ignore())
                .ForMember(x => x.DeveloperAccountsLed, o => o.Ignore());


        }
    }
}