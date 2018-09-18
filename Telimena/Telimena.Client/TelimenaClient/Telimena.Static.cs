using System;
using System.Collections.Generic;
using System.Net.Http;
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
        public static Task<StatisticsUpdateResponse> SendUsageReport([CallerMemberName] string functionName = null, Uri telemetryApiBaseUrl = null
            , Assembly mainAssembly = null, bool suppressAllErrors = true)
        {
            if (telemetryApiBaseUrl == null)
            {
                telemetryApiBaseUrl = defaultApiUri;
            }

            TelimenaHttpClient httpClient = new TelimenaHttpClient(new HttpClient {BaseAddress = telemetryApiBaseUrl});
            return SendUsageReport(httpClient, null, mainAssembly, suppressAllErrors, functionName);
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
        public static Task<StatisticsUpdateResponse> SendUsageReport(ProgramInfo programInfo, Uri telemetryApiBaseUrl = null, Assembly mainAssembly = null
            , bool suppressAllErrors = true, [CallerMemberName] string functionName = null)
        {
            if (telemetryApiBaseUrl == null)
            {
                telemetryApiBaseUrl = defaultApiUri;
            }

            TelimenaHttpClient httpClient = new TelimenaHttpClient(new HttpClient {BaseAddress = telemetryApiBaseUrl});
            return SendUsageReport(httpClient, programInfo, mainAssembly, suppressAllErrors, functionName);
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
        internal static async Task<StatisticsUpdateResponse> SendUsageReport(ITelimenaHttpClient httpClient, ProgramInfo programInfo = null
            , Assembly mainAssembly = null, bool suppressAllErrors = true, [CallerMemberName] string functionName = null)
        {
            RegistrationRequest registrationRequest = null;
            StatisticsUpdateRequest updateRequest = null;
            try
            {
                Tuple<ProgramInfo, UserInfo, string> data = LoadProgramData(mainAssembly, programInfo);


                TelimenaSerializer serializer = new TelimenaSerializer();
                Messenger messenger = new Messenger(serializer, httpClient, suppressAllErrors);

                registrationRequest = new RegistrationRequest
                {
                    ProgramInfo = data.Item1, TelimenaVersion = data.Item3, UserInfo = data.Item2, SkipUsageIncrementation = true
                };
                string responseContent = await messenger.SendPostRequest(ApiRoutes.RegisterClient, registrationRequest);
                RegistrationResponse registrationResponse = serializer.Deserialize<RegistrationResponse>(responseContent);

                updateRequest = new StatisticsUpdateRequest
                {
                    ProgramId = registrationResponse.ProgramId
                    , UserId = registrationResponse.UserId
                    , FunctionName = functionName
                    , Version = data.Item1.PrimaryAssembly.Version
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

        private static Tuple<ProgramInfo, UserInfo, string> LoadProgramData(Assembly mainAssembly = null, ProgramInfo programInfo = null)
        {
            Assembly assembly = mainAssembly ?? Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            ProgramInfo info = programInfo;
            if (info == null)
            {
                info = new ProgramInfo {PrimaryAssembly = new AssemblyInfo(assembly), Name = assembly.GetName().Name};
            }

            UserInfo userInfo = new UserInfo {UserName = Environment.UserName, MachineName = Environment.MachineName};

            string telimenaVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            return new Tuple<ProgramInfo, UserInfo, string>(info, userInfo, telimenaVersion);
        }
    }
}