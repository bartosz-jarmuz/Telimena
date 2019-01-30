using System.Collections.Generic;

namespace Telimena.WebApp.Core.DTO.MappableToClient
{
    /// <summary>
    ///     A data about a program
    /// </summary>
    public class ProgramInfo
    {

        /// <summary>
        ///     A typical program has a primary assembly or an 'entry point'.
        ///     This is where it's info should be defined
        /// </summary>
        public AssemblyInfo PrimaryAssembly { get; set; }

        /// <summary>
        ///     The name of the application.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     An optional collection of helper assemblies data
        /// </summary>
        public List<AssemblyInfo> HelperAssemblies { get; set; }

        /// <summary>
        /// Gets the primary assembly path.
        /// </summary>
        /// <value>The primary assembly path.</value>
        public string PrimaryAssemblyPath => this.PrimaryAssembly?.Location;

    }
}