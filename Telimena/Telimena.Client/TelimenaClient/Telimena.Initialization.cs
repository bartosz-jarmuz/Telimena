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
        public async Task<TelemetryInitializeResponse> InitializeAsync(Dictionary<string, string> telemetryData = null)
        {
            TelemetryInitializeRequest request = null;
            try
            {
                request = new TelemetryInitializeRequest(this.TelemetryKey)
                {
                    ProgramInfo = this.StaticProgramInfo
                    ,TelimenaVersion = this.TelimenaVersion
                    ,UserInfo = this.UserInfo
                    ,SkipUsageIncrementation = false
                };
                string responseContent = await this.Messenger.SendPostRequest(ApiRoutes.Initialize, request).ConfigureAwait(false);
                TelemetryInitializeResponse response = this.Serializer.Deserialize<TelemetryInitializeResponse>(responseContent);
                return response;
            }

            catch (Exception ex)
            {
                TelimenaException exception = new TelimenaException("Error occurred while sending registration request", ex
                    , new KeyValuePair<Type, object>(typeof(TelemetryUpdateRequest), request));
                if (!this.SuppressAllErrors)
                {
                    throw exception;
                }

                return new TelemetryInitializeResponse { Exception = exception };
            }
        }

        /// <inheritdoc />
        public TelemetryInitializeResponse InitializeBlocking(Dictionary<string, string> telemetryData = null)
        {
            return Task.Run(() => this.InitializeAsync(telemetryData)).GetAwaiter().GetResult();
        }


        /// <inheritdoc />
        public async Task<TelemetryInitializeResponse> InitializeAsync_toReDo()
        {
            var response = await this.InitializeAsync().ConfigureAwait(false);
            
            await this.LoadLiveData(response).ConfigureAwait(false);

            this.Locator = new Locator(this.LiveProgramInfo);
            return response;
        }

        /// <inheritdoc />
        public TelemetryInitializeResponse InitializeBlocking_toReDo()
        {
            return Task.Run(this.InitializeAsync_toReDo).GetAwaiter().GetResult();
        }

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

        private async Task InitializeIfNeeded()
        {
            if (!this.IsInitialized)
            {
                var response = await this.InitializeAsync_toReDo().ConfigureAwait(false);
                if (response.Exception == null)
                {
                    this.IsInitialized = true;
                }
            }
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
                this.LiveProgramInfo = new LiveProgramInfo(this.StaticProgramInfo)
                {
                    UserId = response.UserId
                };

                Task<string> updaterNameTask = this.Messenger.SendGetRequest($"{ApiRoutes.GetProgramUpdaterName}?{ApiRoutes.QueryParams.TelemetryKey}={this.TelemetryKey}");

                await Task.WhenAll(updaterNameTask).ConfigureAwait(false);

                this.LiveProgramInfo.UpdaterName = updaterNameTask.Result;

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