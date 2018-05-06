namespace Telimena.WebApp.Infrastructure.DTO
{
    using System.Reflection;

        public class ProgramInfoDto
        {
            public Assembly MainAssembly { get; internal set; }
            public string Name { get; internal set; }
            public string Version { get; internal set; }
        }
}