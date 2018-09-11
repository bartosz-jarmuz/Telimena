using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Telimena.Client
{
    #region Using

    #endregion


    /// <summary>
    ///     Telemetry and Lifecycle Management Engine App
    ///     <para>This is a client SDK that allows handling application telemetry and lifecycle</para>
    /// </summary>
    public partial class Telimena : ITelimena
    {
        public async Task<UpdateCheckResult> CheckForUpdates()
        {
            try
            {
                UpdateResponse response = await this.GetUpdateResponse();
                return new UpdateCheckResult
                {
                    UpdatesToInstall = response.UpdatePackagesIncludingBeta
                };
            }
            catch (Exception ex)
            {
                TelimenaException exception = new TelimenaException($"Error occurred while sending check for updates request", ex,
                    new KeyValuePair<Type, object>(typeof(string), this.GetUpdateRequestUrl()));
                if (!this.SuppressAllErrors)
                {
                    throw exception;
                }
                return new UpdateCheckResult()
                {
                    Error = exception
                };
            }
        }

        public async Task<RegistrationResponse> Initialize()
        {
            return await this.RegisterClient();
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

        /// <summary>
        ///     Report the usage of the application function.
        /// </summary>
        /// <param name="functionName"></param>
        /// <returns></returns>
        public async Task<StatisticsUpdateResponse> ReportUsage([CallerMemberName] string functionName = null)
        {
            StatisticsUpdateRequest request = null;
            try
            {
                await this.InitializeIfNeeded();
                request = new StatisticsUpdateRequest
                {
                    ProgramId = this.ProgramId,
                    UserId = this.UserId,
                    FunctionName = functionName,
                    Version = this.ProgramInfo.PrimaryAssembly.Version
                };
                string responseContent = await this.Messenger.SendPostRequest(ApiRoutes.UpdateProgramStatistics, request);
                return this.Serializer.Deserialize<StatisticsUpdateResponse>(responseContent);
            }
            catch (Exception ex)
            {
                TelimenaException exception = new TelimenaException($"Error occurred while sending update [{functionName}] statistics request", ex,
                    new KeyValuePair<Type, object>(typeof(StatisticsUpdateRequest), request));
                if (!this.SuppressAllErrors)
                {
                    throw exception;
                }
                return new StatisticsUpdateResponse()
                {
                    Error = exception
                };
            }
           
        }


        public async Task HandleUpdates(BetaVersionSettings betaVersionSettings)
        {
            try
            {
                UpdateResponse response = await this.GetUpdateResponse();

                UpdateHandler handler = new UpdateHandler(this.Messenger, this.ProgramInfo, this.SuppressAllErrors, new DefaultWpfInputReceiver(),
                    new UpdateInstaller());
                await handler.HandleUpdates(response, betaVersionSettings);
            }
            catch (Exception ex)
            {
                TelimenaException exception = new TelimenaException($"Error occurred while handling updates", ex,
                    new KeyValuePair<Type, object>(typeof(string), this.GetUpdateRequestUrl()));
                if (!this.SuppressAllErrors)
                {
                    throw exception;
                }
            }
        }

        private string GetUpdateRequestUrl()
        {
            return ApiRoutes.GetUpdatesInfo + "?programId=" + this.ProgramId + "&version=" + this.ProgramVersion;
        }

        protected async Task<UpdateResponse> GetUpdateResponse()
        {
            await this.InitializeIfNeeded();
            string responseContent =
                await this.Messenger.SendGetRequest(this.GetUpdateRequestUrl());
            return this.Serializer.Deserialize<UpdateResponse>(responseContent);
        }

        

        /// <summary>
        ///     Sends the initial app usage info
        /// </summary>
        /// <returns></returns>
        protected internal async Task<RegistrationResponse> RegisterClient(bool skipUsageIncrementation = false)
        {
            RegistrationRequest request = null;
            try
            {
                request = new RegistrationRequest
                {
                    ProgramInfo = this.ProgramInfo,
                    TelimenaVersion = this.TelimenaVersion,
                    UserInfo = this.UserInfo,
                    SkipUsageIncrementation = skipUsageIncrementation
                };
                string responseContent = await this.Messenger.SendPostRequest(ApiRoutes.RegisterClient, request);
                RegistrationResponse response = this.Serializer.Deserialize<RegistrationResponse>(responseContent);
                this.UserId = response.UserId;
                this.ProgramId = response.ProgramId;
                return response;
            }

            catch (Exception ex)
            {
                TelimenaException exception = new TelimenaException($"Error occurred while sending registration request", ex,
                    new KeyValuePair<Type, object>(typeof(StatisticsUpdateRequest), request));
                if (!this.SuppressAllErrors)
                {
                    throw exception;
                }
                return new RegistrationResponse()
                {
                    Error = exception
                };
            }
        }

        private async Task InitializeIfNeeded()
        {
            if (!this.IsInitialized)
            {
                this.IsInitialized = true;
                await this.Initialize();
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