namespace Telimena.Client
{
    using System.Collections.Generic;
    using System.Diagnostics;

    public class ProgramInfo
    {
        public AssemblyInfo PrimaryAssembly { get;  set; }
        public string Name { get;  set; }
        public int? DeveloperId { get; set; }

        public List<AssemblyInfo> HelperAssemblies { get; set; }

    }
}