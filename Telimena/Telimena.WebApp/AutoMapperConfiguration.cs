using AutoMapper;
using Telimena.WebApp.Infrastructure;

namespace Telimena.WebApp
{
    /// <summary>
    /// Class AutoMapperConfiguration.
    /// </summary>
    public static class AutoMapperConfiguration
    {
        /// <summary>
        /// Configures this instance.
        /// </summary>
        public static void Configure()
        {
            Mapper.Initialize(cfg => cfg.AddProfiles(typeof(AutoMapperWebProfile), typeof(AutoMapperDomainProfile)));
        }

        /// <summary>
        /// Validates this instance.
        /// </summary>
        public static void Validate()
        {
            Mapper.AssertConfigurationIsValid();
        }
    }
}