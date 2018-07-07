using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Telimena.WebApp
{
    using AutoMapper;
    using Client;
    using Infrastructure;
    using Infrastructure.Repository;
    using WebApi.Controllers;

    public static class AutoMapperConfiguration
    {
        public static void Configure()
        {
            Mapper.Initialize(cfg =>
                    cfg.AddProfiles(new[] {
                        typeof(AutoMapperWebProfile),
                        typeof(AutoMapperDomainProfile)
                    }));
        }
        public static void Validate()
        {
            Mapper.AssertConfigurationIsValid();
        }
    }
}