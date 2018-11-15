using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace TelimenaClient
{
    #region Using

    #endregion

    /// <summary>
    ///     Telemetry and Lifecycle Management Engine App
    ///     <para>This is a client SDK that allows handling application telemetry and lifecycle</para>
    /// </summary>
    public partial interface ITelimena
    {
        /// <summary>
        ///     Loads the referenced helper assemblies, e.g. for the purpose of updating
        /// </summary>
        /// <param name="assemblies"></param>
        void LoadHelperAssemblies(params Assembly[] assemblies);

        /// <summary>
        ///     Loads the referenced helper assemblies, e.g. for the purpose of updating
        /// </summary>
        /// <param name="assemblyNames"></param>
        void LoadHelperAssembliesByName(params string[] assemblyNames);

        /// <summary>
        /// If true, then Telimena will swallow any errors. Otherwise, it will rethrow
        /// </summary>
        bool SuppressAllErrors { get; set; }
    }
}