using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// <param name="functionName"></param>
        /// <param name="telemetryApiBaseUrl">Base url of the telemetry api</param>
        /// <param name="mainAssembly"></param>
        /// <param name="suppressAllErrors"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Task<StatisticsUpdateResponse> ReportUsageStatic([CallerMemberName] string functionName = null, Uri telemetryApiBaseUrl = null
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
            return ReportUsageStatic(httpClient, null, mainAssembly, suppressAllErrors, functionName);
        }

        /// <summary>
        ///     Sends the usage report. The static method first calls a 'register' endpoint, without incrementing the usage, then
        ///     sends the usage report
        ///     <para />
        ///     Static method is approximately two times slower than instance based
        /// </summary>
        /// <param name="programInfo"></param>
        /// <param name="telemetryApiBaseUrl"></param>
        /// <param name="mainAssembly"></param>
        /// <param name="suppressAllErrors"></param>
        /// <param name="functionName"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Task<StatisticsUpdateResponse> ReportUsageStatic(ProgramInfo programInfo, Uri telemetryApiBaseUrl = null, Assembly mainAssembly = null
            , bool suppressAllErrors = true, [CallerMemberName] string functionName = null)
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
            return ReportUsageStatic(httpClient, programInfo, mainAssembly, suppressAllErrors, functionName);
        }

        /// <summary>
        ///     Sends the usage report. The static method first calls a 'register' endpoint, without incrementing the usage, then
        ///     sends the usage report
        ///     <para />
        ///     Static method is approximately two times slower than instance based
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="programInfo"></param>
        /// <param name="mainAssembly"></param>
        /// <param name="suppressAllErrors"></param>
        /// <param name="functionName"></param>
        /// <returns></returns>
        internal static async Task<StatisticsUpdateResponse> ReportUsageStatic(ITelimenaHttpClient httpClient, ProgramInfo programInfo = null
            , Assembly mainAssembly = null, bool suppressAllErrors = true, [CallerMemberName] string functionName = null)
        {
            RegistrationRequest registrationRequest = null;
            StatisticsUpdateRequest updateRequest = null;

            if (mainAssembly == null)
            {
                mainAssembly = GetProperCallingAssembly();
            }

            try
            {
               var data = LoadProgramData(mainAssembly, programInfo);


                TelimenaSerializer serializer = new TelimenaSerializer();
                Messenger messenger = new Messenger(serializer, httpClient);

                registrationRequest = new RegistrationRequest
                {
                    ProgramInfo = data.ProgramInfo, TelimenaVersion = data.TelimenaVersion, UserInfo = data.UserInfo, SkipUsageIncrementation = true
                };
                string responseContent = await messenger.SendPostRequest(ApiRoutes.RegisterClient, registrationRequest);
                RegistrationResponse registrationResponse = serializer.Deserialize<RegistrationResponse>(responseContent);

                updateRequest = new StatisticsUpdateRequest
                {
                    ProgramId = registrationResponse.ProgramId
                    , UserId = registrationResponse.UserId
                    , FunctionName = functionName
                    , Version = data.ProgramInfo.PrimaryAssembly.Version
                };
                responseContent = await messenger.SendPostRequest(ApiRoutes.UpdateProgramStatistics, updateRequest);
                return serializer.Deserialize<StatisticsUpdateResponse>(responseContent);
            }
            catch (Exception ex)
            {
                TelimenaException exception = new TelimenaException($"Error occurred while sending update [{functionName}] statistics request", ex
                    , new KeyValuePair<Type, object>(typeof(RegistrationRequest), registrationRequest)
                    , new KeyValuePair<Type, object>(typeof(StatisticsUpdateRequest), updateRequest));
                if (!suppressAllErrors)
                {
                    throw exception;
                }

                return new StatisticsUpdateResponse {Exception = exception};
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

            string telimenaVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string updaterVersion = GetUpdaterVersion(info);

            return new StartupData(info, userInfo, telimenaVersion, updaterVersion);
        }
        private static string GetUpdaterVersion(ProgramInfo programInfo)
        {
            var updaterFile = UpdateHandler.PathFinder.GetUpdaterExecutable(UpdateHandler.BasePath, UpdateHandler.GetUpdatesFolderName(programInfo));
            if (updaterFile.Exists)
            {
                var version = FileVersionInfo.GetVersionInfo(updaterFile.FullName);
                return version.FileVersion;
            }
            else
            {
                return "0.0.0.0";
            }
        }
    }
}