namespace Telimena.Client
{
    #region Using
    using System;
    using System.Net.Http;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    #endregion

    /// <summary>
    ///     Telemetry and Lifecycle Management Engine App
    ///     <para>This is a client SDK that allows handling application telemetry and lifecycle</para>
    /// </summary>
    public partial class Telimena : ITelimena
    {


       /// <summary>
       /// Sends the usage report. The static method first calls a 'register' endpoint, without incrementing the usage, then sends the usage report <para/>
       /// Static method is approximately two times slower than instance based
       /// </summary>
       /// <param name="telemetryApiBaseUrl">Base url of the telemetry api</param>
       /// <param name="mainAssembly"></param>
       /// <param name="suppressAllErrors"></param>
       /// <param name="functionName"></param>
       /// <returns></returns>
        public static Task<StatisticsUpdateResponse> SendUsageReport(string telemetryApiBaseUrl = Telimena.DefaultApiUri, Assembly mainAssembly = null, bool suppressAllErrors = true, [CallerMemberName] string functionName = null)
        {
            TelimenaHttpClient httpClient = new TelimenaHttpClient(new HttpClient()
            {
                BaseAddress = new Uri(telemetryApiBaseUrl)
            });
            return Telimena.SendUsageReport(httpClient, null, mainAssembly, suppressAllErrors, functionName);
        }

        public static Task<StatisticsUpdateResponse> SendUsageReport(ProgramInfo programInfo, string telemetryApiBaseUrl = Telimena.DefaultApiUri, Assembly mainAssembly = null, bool suppressAllErrors = true, [CallerMemberName] string functionName = null)
        {
            TelimenaHttpClient httpClient = new TelimenaHttpClient(new HttpClient()
            {
                BaseAddress = new Uri(telemetryApiBaseUrl)
            });
            return Telimena.SendUsageReport(httpClient, programInfo, mainAssembly, suppressAllErrors, functionName);
        }

        /// <summary>
        /// Sends the usage report. The static method first calls a 'register' endpoint, without incrementing the usage, then sends the usage report <para/>
        /// Static method is approximately two times slower than instance based
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="programInfo"></param>
        /// <param name="mainAssembly"></param>
        /// <param name="suppressAllErrors"></param>
        /// <param name="functionName"></param>
        /// <returns></returns>
        internal static async Task<StatisticsUpdateResponse> SendUsageReport(ITelimenaHttpClient httpClient, ProgramInfo programInfo= null, Assembly mainAssembly = null, bool suppressAllErrors = true, [CallerMemberName] string functionName = null)
        {
            try
            {
                Tuple<ProgramInfo, UserInfo, string> data = Telimena.LoadProgramData(mainAssembly, programInfo);

               
                TelimenaSerializer serializer = new TelimenaSerializer();
                Messenger messenger = new Messenger(serializer, httpClient, suppressAllErrors);

                RegistrationRequest registrationRequest = new RegistrationRequest()
                {
                    ProgramInfo = data.Item1,
                    TelimenaVersion = data.Item3,
                    UserInfo = data.Item2,
                    SkipUsageIncrementation = true
                };
                string responseContent = await messenger.SendPostRequest(ApiRoutes.RegisterClient, registrationRequest);
                RegistrationResponse registrationResponse = serializer.Deserialize<RegistrationResponse>(responseContent);

                StatisticsUpdateRequest updateRequest = new StatisticsUpdateRequest()
                {
                    ProgramId = registrationResponse.ProgramId,
                    UserId = registrationResponse.UserId,
                    FunctionName = functionName,
                    Version = data.Item1.PrimaryAssembly.Version
                };
                responseContent = await messenger.SendPostRequest(ApiRoutes.UpdateProgramStatistics, updateRequest);
                return serializer.Deserialize<StatisticsUpdateResponse>(responseContent);
            }
            catch (Exception ex)
            {
                if (!suppressAllErrors)
                {
                    throw;
                }
                return new StatisticsUpdateResponse()
                {
                    Error = ex
                };
            }
        }

        private static Tuple<ProgramInfo, UserInfo, string> LoadProgramData(Assembly mainAssembly = null, ProgramInfo programInfo = null)
        {
            Assembly assembly = mainAssembly ?? Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            ProgramInfo info = programInfo;
            if (info == null)
            {
                info = new ProgramInfo()
                {
                    PrimaryAssembly = new AssemblyInfo(assembly),
                    Name = assembly.GetName().Name,
                };
            }
        
            UserInfo userInfo = new UserInfo()
            {
                UserName = Environment.UserName,
                MachineName = Environment.MachineName
            };

            string telimenaVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            return new Tuple<ProgramInfo, UserInfo, string>(info, userInfo, telimenaVersion);
        }
    }
}