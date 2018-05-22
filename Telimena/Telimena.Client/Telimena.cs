namespace Telimena.Client
{
    #region Using
    using System;
    using System.Net.Http;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;
    #endregion

    /// <summary>
    ///     Telemetry and Lifecycle Management Engine App
    ///     <para>This is a client SDK that allows handling application telemetry and lifecycle</para>
    /// </summary>
    public partial class Telimena : ITelimena
    {
        public async Task<UpdateResponse> CheckForUpdates()
        {
            await this.InitializeIfNeeded();

            throw new NotImplementedException();
        }

        public async Task<RegistrationResponse> Initialize()
        {
            return await this.RegisterClient();
        }

        /// <summary>
        ///     Report the usage of the application function.
        /// </summary>
        /// <param name="functionName"></param>
        /// <returns></returns>
        public async Task<StatisticsUpdateResponse> ReportUsage([CallerMemberName] string functionName = null)
        {
            try
            {
                await this.InitializeIfNeeded();
                StatisticsUpdateRequest request = new StatisticsUpdateRequest()
                {
                    ProgramId = this.ProgramId,
                    UserId = this.UserId,
                    FunctionName = functionName
                };
                string responseContent = await this.SendPostRequest(ApiRoutes.UpdateProgramStatistics, request);
                return this.Serializer.Deserialize<StatisticsUpdateResponse>(responseContent);
            }
            catch (Exception ex)
            {
                if (!this.SuppressAllErrors)
                {
                    throw;
                }

                return new StatisticsUpdateResponse()
                {
                    Error = ex
                };
            }
        }

        /// <summary>
        ///     Sends the initial app usage info
        /// </summary>
        /// <returns></returns>
        protected async Task<RegistrationResponse> RegisterClient()
        {
            try
            {
                RegistrationRequest request = new RegistrationRequest()
                {
                    ProgramInfo = this.ProgramInfo,
                    TelimenaVersion = this.TelimenaVersion,
                    UserInfo = this.UserInfo
                };
                string responseContent = await this.SendPostRequest(ApiRoutes.RegisterClient, request);
                RegistrationResponse response = this.Serializer.Deserialize<RegistrationResponse>(responseContent);
                this.UserId = response.UserId;
                this.ProgramId = response.ProgramId;
                return response;
            }
            catch (Exception ex)
            {
                if (!this.SuppressAllErrors)
                {
                    throw;
                }

                return new RegistrationResponse()
                {
                    Error = ex
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

        private async Task<string> SendPostRequest(string requestUri, object objectToPost)
        {
            try
            {
                string jsonObject = this.Serializer.Serialize(objectToPost);
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
    }
}