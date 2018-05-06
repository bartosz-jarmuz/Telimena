using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Telimena.WebApp
{
    using AutoMapper;
    using Core.Models;
    using Infrastructure.DTO;


    public class AutoMapperDomainProfile : Profile
    {
        public AutoMapperDomainProfile()
        {
            this.CreateMap<UserInfoDto, UserInfo>().ForMember(x=>x.Id, o=>o.Ignore());
            this.CreateMap<ProgramInfoDto, Program>().
                ForMember(x=>x.Assemblies, o=>o.Ignore()).
                ForMember(x=>x.Developer, o=>o.Ignore()).
                ForMember(x=>x.DeveloperId, o=>o.Ignore()).
                ForMember(x=>x.Id, o=>o.Ignore()).
                ForMember(x=>x.Description, o=>o.Ignore());
        }
    }
}