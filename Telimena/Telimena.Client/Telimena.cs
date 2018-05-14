using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telimena.Client
{
    using System;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Web.Script.Serialization;

    /// <summary>
    /// Telemetry and Lifecycle Management Engine App
    /// <para>This is a client SDK that allows handling application telemetry and lifecycle</para>
    /// </summary>
    public class Telimena : ITelimena
    {
        /// <summary>
        /// Creates a new instance of Telimena Client
        /// </summary>
        /// <param name="telemetryApiBaseUrl">Leave default, unless you want to call different telemetry server</param>
        /// <param name="mainAssembly">Leave null, unless you want to use different assembly as the main one for program name, version etc</param>
        public Telimena(string telemetryApiBaseUrl = "http://localhost:7757/",
                        Assembly mainAssembly = null)
        {
            var assembly = mainAssembly??Assembly.GetEntryAssembly()??Assembly.GetExecutingAssembly();
            this.ProgramInfo = new ProgramInfo()
            {
                MainAssembly = new AssemblyInfo(assembly),
                Name = assembly.GetName().Name,
                Version = assembly.GetName().Version.ToString()
            };
            this.UserInfo = new UserInfo()
            {
                UserName = Environment.UserName,
                MachineName = Environment.MachineName
            };

            this.TelimenaVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            this.HttpClient = new HttpClient()
            {
                BaseAddress = new Uri(telemetryApiBaseUrl)
            };
        }


        protected UserInfo UserInfo { get; }
        protected ProgramInfo ProgramInfo { get; }

        public bool SuppressAllErrors { get; set; } = true;
  
        protected string TelimenaVersion { get;  }

        private HttpClient HttpClient { get; } 

        private async Task<string> SendGetRequest(string requestUri)
        {
            try
            {
                HttpResponseMessage response = await this.HttpClient.GetAsync(requestUri);
                 string content = await response.Content.ReadAsStringAsync();
                return content;
            }
            catch (Exception)
            {
                if (!this.SuppressAllErrors)
                {
                    throw;
                }

                return "";
            }
        }

        private async Task<string> SendPostRequest(string requestUri, object objectToPost)
        {
            try
            {
                string jsonObject = this.Serialize(objectToPost);
                StringContent content = new StringContent(jsonObject, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await this.HttpClient.PostAsync(requestUri, content);
                string responseContent = await response.Content.ReadAsStringAsync();
                return responseContent;
            }
            catch (Exception)
            {
                if (!this.SuppressAllErrors)
                {
                    throw;
                }

                return "";
            }
        }

        private string Serialize(object objectToPost)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string jsonObject = serializer.Serialize(objectToPost);
            return jsonObject;
        }

        private T Deserialize<T>(string stringContent)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Deserialize<T>(stringContent);
        }

        /// <summary>
        /// Report the usage of the application.
        /// </summary>
        /// <param name="functionName"></param>
        /// <returns></returns>
        public async Task<StatisticsUpdateResponse> ReportUsage([CallerMemberName] string functionName = null)
        {
            try
            {
                var request = new StatisticsUpdateRequest()
                {
                    FunctionName = functionName,
                    ProgramInfo = this.ProgramInfo,
                    TelimenaVersion = this.TelimenaVersion,
                    UserInfo = this.UserInfo
                };
                var responseContent = await this.SendPostRequest(ApiRoutes.UpdateProgramStatistics, request);
                 return this.Deserialize<StatisticsUpdateResponse>(responseContent);
            }
            catch (Exception ex)
            {
                if (!this.SuppressAllErrors)
                {
                    throw;
                }
                return new StatisticsUpdateResponse(ex);
            }
        }

        public UpdateResponse CheckForUpdates()
        {
            throw new System.NotImplementedException();
        }

        public RegistrationResponse RegisterClient()
        {
            return null;
        }

        public Task Initialize()
        {
            return Task.FromResult(true);
        }
    }
}
