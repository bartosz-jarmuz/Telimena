using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TelimenaClient.Serializer;

namespace TelimenaClient
{

    /// <summary>
    ///     Telemetry and Lifecycle Management Engine App
    ///     <para>This is a client SDK that allows handling application telemetry and lifecycle</para>
    /// </summary>
    public partial class Telimena : ITelimena
    {
        /// <summary>
        ///     Sends the usage report. The static method first calls a 'register' endpoint, without incrementing the usage, then
        ///     sends the usage report
        ///     <para />
        ///     Static method is approximately two times slower than instance based
        /// </summary>
        /// <param name="telemetryKey">The unique key for the telemetry service for this app</param>
        /// <param name="viewName"></param>
        /// <param name="telemetryApiBaseUrl">Base url of the telemetry api</param>
        /// <param name="mainAssembly"></param>
        /// <param name="suppressAllErrors"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Task<TelemetryUpdateResponse> ReportUsageStatic(Guid telemetryKey, [CallerMemberName] string viewName = null, Uri telemetryApiBaseUrl = null
            , Assembly mainAssembly = null, bool suppressAllErrors = true)
        {
            if (telemetryApiBaseUrl == null)
            {
                telemetryApiBaseUrl = DefaultApiUri;
            }
              
            if (mainAssembly == null)
            {
                mainAssembly = GetProperCallingAssembly();
            }

            TelimenaHttpClient httpClient = new TelimenaHttpClient(new HttpClient {BaseAddress = telemetryApiBaseUrl});
            return ReportUsageStatic(telemetryKey, httpClient, null, mainAssembly, suppressAllErrors, viewName);
        }

        /// <summary>
        ///     Sends the usage report. The static method first calls a 'register' endpoint, without incrementing the usage, then
        ///     sends the usage report
        ///     <para />
        ///     Static method is approximately two times slower than instance based
        /// </summary>
        /// <param name="telemetryKey">The unique key for the telemetry service for this app</param>
        /// <param name="programInfo"></param>
        /// <param name="telemetryApiBaseUrl"></param>
        /// <param name="mainAssembly"></param>
        /// <param name="suppressAllErrors"></param>
        /// <param name="viewName"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Task<TelemetryUpdateResponse> ReportUsageStatic(Guid telemetryKey, ProgramInfo programInfo, Uri telemetryApiBaseUrl = null, Assembly mainAssembly = null
            , bool suppressAllErrors = true, [CallerMemberName] string viewName = null)
        {
            if (telemetryApiBaseUrl == null)
            {
                telemetryApiBaseUrl = DefaultApiUri;
            }

            if (mainAssembly == null)
            {
                mainAssembly = GetProperCallingAssembly();
            }
            TelimenaHttpClient httpClient = new TelimenaHttpClient(new HttpClient {BaseAddress = telemetryApiBaseUrl});
            return ReportUsageStatic(telemetryKey, httpClient, programInfo, mainAssembly, suppressAllErrors, viewName);
        }

        /// <summary>
        ///     Sends the usage report. The static method first calls a 'register' endpoint, without incrementing the usage, then
        ///     sends the usage report
        ///     <para />
        ///     Static method is approximately two times slower than instance based
        /// </summary>
        /// <param name="telemetryKey"></param>
        /// <param name="httpClient"></param>
        /// <param name="programInfo"></param>
        /// <param name="mainAssembly"></param>
        /// <param name="suppressAllErrors"></param>
        /// <param name="viewName"></param>
        /// <returns></returns>
        internal static async Task<TelemetryUpdateResponse> ReportUsageStatic(Guid telemetryKey, ITelimenaHttpClient httpClient, ProgramInfo programInfo = null
            , Assembly mainAssembly = null, bool suppressAllErrors = true, [CallerMemberName] string viewName = null)
        {
            TelemetryInitializeRequest telemetryInitializeRequest = null;
            TelemetryUpdateRequest updateRequest = null;

            if (mainAssembly == null)
            {
                mainAssembly = GetProperCallingAssembly();
            }

            try
            {
               var data = LoadProgramData(mainAssembly, programInfo);


                TelimenaSerializer serializer = new TelimenaSerializer();
                Messenger messenger = new Messenger(serializer, httpClient);

                telemetryInitializeRequest = new TelemetryInitializeRequest(telemetryKey)
                {
                    ProgramInfo = data.ProgramInfo, TelimenaVersion = data.TelimenaVersion, UserInfo = data.UserInfo, SkipUsageIncrementation = true
                };
                string responseContent = await messenger.SendPostRequest(ApiRoutes.Initialize, telemetryInitializeRequest).ConfigureAwait(false);
                TelemetryInitializeResponse telemetryInitializeResponse = serializer.Deserialize<TelemetryInitializeResponse>(responseContent);

                updateRequest = new TelemetryUpdateRequest(telemetryKey)
                {
                     UserId = telemetryInitializeResponse.UserId
                    , ComponentName = viewName
                    , VersionData = data.ProgramInfo.PrimaryAssembly.VersionData
                };
                responseContent = await messenger.SendPostRequest(ApiRoutes.UpdateProgramStatistics, updateRequest).ConfigureAwait(false);
                return serializer.Deserialize<TelemetryUpdateResponse>(responseContent);
            }
            catch (Exception ex)
            {
                TelimenaException exception = new TelimenaException($"Error occurred while sending update [{viewName}] statistics request", ex
                    , new KeyValuePair<Type, object>(typeof(TelemetryInitializeRequest), telemetryInitializeRequest)
                    , new KeyValuePair<Type, object>(typeof(TelemetryUpdateRequest), updateRequest));
                if (!suppressAllErrors)
                {
                    throw exception;
                }

                return new TelemetryUpdateResponse {Exception = exception};
            }
        }

        private static Assembly GetProperCallingAssembly()
        {
            StackTrace stackTrace = new StackTrace();
            int index = 1;
            AssemblyName currentAss = typeof(Telimena).Assembly.GetName();
            while (true)
            {
                MethodBase method = stackTrace.GetFrame(index)?.GetMethod();
                if (method?.DeclaringType?.Assembly.GetName().Name != currentAss.Name)
                {
                    if (method?.DeclaringType?.Assembly?.GetName()?.Name != "mscorlib")
                    {
                        return method.DeclaringType.Assembly;
                    }
                }
                index++;
            }
        }
        private static StartupData LoadProgramData(Assembly assembly, ProgramInfo programInfo = null)
        {
            ProgramInfo info = programInfo;
            if (info == null)
            {
                info = new ProgramInfo {PrimaryAssembly = new AssemblyInfo(assembly), Name = assembly.GetName().Name};
            }

            UserInfo userInfo = new UserInfo {UserName = Environment.UserName, MachineName = Environment.MachineName};

            string telimenaVersion = TelimenaVersionReader.ReadToolkitVersion(Assembly.GetExecutingAssembly());

            return new StartupData(info, userInfo, telimenaVersion);
        }
      
    }
}