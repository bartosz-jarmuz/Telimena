namespace Telimena.Client
{
    using System.Reflection;

    public class ProgramInfo
    {
        public Assembly MainAssembly { get; internal set; }
        public string Name { get; internal set; }
        public string Version { get; internal set; }
    }
}