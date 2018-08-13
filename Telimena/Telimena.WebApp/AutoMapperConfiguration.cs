namespace Telimena.WebApp
{
    using AutoMapper;
    using Infrastructure;

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