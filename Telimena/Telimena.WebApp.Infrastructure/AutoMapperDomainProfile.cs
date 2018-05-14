using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Telimena.WebApp
{
    using AutoMapper;
    using Client;
    using Core.Models;


    public class AutoMapperDomainProfile : Profile
    {
        public AutoMapperDomainProfile()
        {
            this.CreateMap<UserInfo, ClientAppUser>().ForMember(x=>x.Id, o=>o.Ignore());
            this.CreateMap<ProgramInfo, Program>().
                ForMember(x=>x.Assemblies, o=>o.Ignore()).
                ForMember(x=>x.Developer, o=>o.Ignore()).
                ForMember(x=>x.DeveloperId, o=>o.Ignore()).
                ForMember(x=>x.Id, o=>o.Ignore()).
                ForMember(x=>x.Description, o=>o.Ignore());
        }
    }


}