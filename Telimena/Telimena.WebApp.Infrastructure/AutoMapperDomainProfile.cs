namespace Telimena.WebApp.Infrastructure
{
    using AutoMapper;
    using Client;
    using Core.Models;

    public class AutoMapperDomainProfile : Profile
    {
        public AutoMapperDomainProfile()
        {
            this.CreateMap<UserInfo, ClientAppUser>().
                ForMember(x=>x.Id, o=>o.Ignore()).
                ForMember(x=>x.RegisteredDate, o=>o.Ignore());
            this.CreateMap<AssemblyInfo, PrimaryAssembly>().
                ForMember(x => x.Id, o => o.Ignore());
            this.CreateMap<AssemblyInfo, ReferencedAssembly>().
                ForMember(x => x.Id, o => o.Ignore());
            this.CreateMap<ProgramInfo, Program>().
                ForMember(x => x.Assemblies, o => o.Ignore()).
                ForMember(x => x.Developer, o => o.Ignore()).
                ForMember(x=>x.RegisteredDate, o=>o.Ignore()).
                ForMember(x=>x.Id, o=>o.Ignore()).
                ForMember(x=>x.Description, o=>o.Ignore());
        }
    }


}