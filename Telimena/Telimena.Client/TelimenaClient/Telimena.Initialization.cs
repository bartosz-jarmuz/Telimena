using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace TelimenaClient
{
    #region Using

    #endregion

    /// <summary>
    ///     Telemetry and Lifecycle Management Engine App
    ///     <para>This is a client SDK that allows handling application telemetry and lifecycle</para>
    /// </summary>
    public partial class Telimena : ITelimena
    {
        /// <summary>
        /// Initializes the Telimena client. <para />
        /// Each time initialization is called, it will increment the program usage statistics.
        /// It should be called once per application execution
        /// </summary>
        /// <returns>Task&lt;RegistrationResponse&gt;.</returns>
        public async Task<RegistrationResponse> Initialize()
        {
            return await this.RegisterClient().ConfigureAwait(false);
        }

        /// <summary>
        ///     Loads the referenced helper assemblies, e.g. for the purpose of updating
        /// </summary>
        /// <param name="assemblies"></param>
        public void LoadHelperAssemblies(params Assembly[] assemblies)
        {
            foreach (Assembly assembly in assemblies)
            {
                this.HelperAssemblies.Add(assembly);
            }

            this.LoadAssemblyInfos(this.HelperAssemblies);
        }

        /// <summary>
        ///     Loads the referenced helper assemblies, e.g. for the purpose of updating
        /// </summary>
        /// <param name="assemblyNames"></param>
        public void LoadHelperAssembliesByName(params string[] assemblyNames)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            foreach (string assemblyName in assemblyNames)
            {
                Assembly assembly = Assembly.LoadFrom(Path.Combine(path, assemblyName));
                this.HelperAssemblies.Add(assembly);
            }

            this.LoadAssemblyInfos(this.HelperAssemblies);
        }

        private async Task InitializeIfNeeded()
        {
            if (!this.IsInitialized)
            {
                var response = await this.Initialize().ConfigureAwait(false);
                if (response.Exception == null)
                {
                    this.IsInitialized = true;
                }
            }
        }

        private void LoadAssemblyInfos(IEnumerable<Assembly> assemblies)
        {
            this.ProgramInfo.HelperAssemblies = new List<AssemblyInfo>();
            foreach (Assembly assembly in assemblies)
            {
                this.ProgramInfo.HelperAssemblies.Add(new AssemblyInfo(assembly));
            }
        }
    }
}