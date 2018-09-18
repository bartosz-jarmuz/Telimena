using AutoMapper;
using Telimena.WebApp.Infrastructure;

namespace Telimena.WebApp
{
    public static class AutoMapperConfiguration
    {
        public static void Configure()
        {
            Mapper.Initialize(cfg => cfg.AddProfiles(typeof(AutoMapperWebProfile), typeof(AutoMapperDomainProfile)));
        }

        public static void Validate()
        {
            Mapper.AssertConfigurationIsValid();
        }
    }
}