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
        /// <inheritdoc />
        public void LoadHelperAssemblies(params Assembly[] assemblies)
        {
            foreach (Assembly assembly in assemblies)
            {
                this.HelperAssemblies.Add(assembly);
            }

            this.LoadAssemblyInfos(this.HelperAssemblies);
        }

        /// <inheritdoc />
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

        private async Task<TelemetryInitializeResponse> InitializeIfNeeded()
        {
            if (!this.IsInitialized)
            {
                TelemetryInitializeResponse response = await this.Async.Initialize().ConfigureAwait(false);
                if (response != null && response.Exception == null)
                {
                    this.IsInitialized = true;
                    this.initializationResponse = response;
                    return this.initializationResponse;
                }
                else
                {
                    return response;
                }
            }

            return this.initializationResponse;
        }

        private void LoadAssemblyInfos(IEnumerable<Assembly> assemblies)
        {
            this.StaticProgramInfo.HelperAssemblies = new List<AssemblyInfo>();
            foreach (Assembly assembly in assemblies)
            {
                this.StaticProgramInfo.HelperAssemblies.Add(new AssemblyInfo(assembly));
            }
        }

        private async Task LoadLiveData(TelemetryInitializeResponse response)
        {
            try
            {
                this.LiveProgramInfo = new LiveProgramInfo(this.StaticProgramInfo) {UserId = response.UserId};

                Task<string> updaterNameTask =
                    this.Messenger.SendGetRequest($"{ApiRoutes.GetProgramUpdaterName}?{ApiRoutes.QueryParams.TelemetryKey}={this.TelemetryKey}");

                await Task.WhenAll(updaterNameTask).ConfigureAwait(false);

                this.LiveProgramInfo.UpdaterName = updaterNameTask.Result;

                if (string.IsNullOrEmpty(this.LiveProgramInfo.UpdaterName))
                {
                    throw new InvalidOperationException($"Updater name is null or empty. Task result: {updaterNameTask.Status}");
                }
            }
            catch (Exception ex)
            {
                TelimenaException exception = new TelimenaException("Error occurred while loading live program info", ex);
                if (!this.SuppressAllErrors)
                {
                    throw exception;
                }
            }
        }
    }
}