using System;
using System.Collections.Generic;
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
        /// Report the usage of the application function.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="customDataObject">A simple data object to be serialized and sent to Telimena. MUST BE JSON SERIALIZABLE</param>
        /// <param name="functionName">The name of the function. If left blank, it will report the name of the invoked method</param>
        /// <returns>Task&lt;StatisticsUpdateResponse&gt;.</returns>
        /// <exception cref="ArgumentException">Invalid object passed as custom data for telemetry.</exception>
        public Task<StatisticsUpdateResponse> ReportUsageWithCustomData<T>(T customDataObject, [CallerMemberName] string functionName = null)
        {
            string serialized = null;
            if (customDataObject != null)
            {

                try
                {
                    serialized = this.Serializer.Serialize(customDataObject);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException("Invalid object passed as custom data for telemetry.", ex);
                }
            }

            return this.ReportUsageWithCustomData(serialized, functionName);
        }


        /// <summary>
            ///     Report the usage of the application function.
            /// </summary>
            /// <param name="customData">A JSON serialized object which contains some program specific custom data</param>
            /// <param name="functionName"></param>
            /// <returns></returns>
            public async Task<StatisticsUpdateResponse> ReportUsageWithCustomData(string customData, [CallerMemberName] string functionName = null)
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
                    Version = this.ProgramInfo.PrimaryAssembly.Version,
                    CustomData = customData
                };
                string responseContent = await this.Messenger.SendPostRequest(ApiRoutes.UpdateProgramStatistics, request);
                return this.Serializer.Deserialize<StatisticsUpdateResponse>(responseContent);
            }
            catch (Exception ex)
            {
                TelimenaException exception = new TelimenaException($"Error occurred while sending update [{functionName}] statistics request", ex
                    , new KeyValuePair<Type, object>(typeof(StatisticsUpdateRequest), request));
                if (!this.SuppressAllErrors)
                {
                    throw exception;
                }

                return new StatisticsUpdateResponse { Exception = exception };
            }
        }

        /// <summary>
        ///     Report the usage of the application function.
        /// </summary>
        /// <param name="functionName"></param>
        /// <returns></returns>
        public Task<StatisticsUpdateResponse> ReportUsage([CallerMemberName] string functionName = null)
        {
            return this.ReportUsageWithCustomData(null, functionName);
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
                    ProgramInfo = this.ProgramInfo
                    , TelimenaVersion = this.TelimenaVersion
                    , UserInfo = this.UserInfo
                    , SkipUsageIncrementation = skipUsageIncrementation
                };
                string responseContent = await this.Messenger.SendPostRequest(ApiRoutes.RegisterClient, request);
                RegistrationResponse response = this.Serializer.Deserialize<RegistrationResponse>(responseContent);
                this.UserId = response.UserId;
                this.ProgramId = response.ProgramId;
                return response;
            }

            catch (Exception ex)
            {
                TelimenaException exception = new TelimenaException("Error occurred while sending registration request", ex
                    , new KeyValuePair<Type, object>(typeof(StatisticsUpdateRequest), request));
                if (!this.SuppressAllErrors)
                {
                    throw exception;
                }

                return new RegistrationResponse {Exception = exception};
            }
        }
    }
}