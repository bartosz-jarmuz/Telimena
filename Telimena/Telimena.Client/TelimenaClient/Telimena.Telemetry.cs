using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
        public StatisticsUpdateResponse ReportUsageWithCustomDataBlocking(string customData, [CallerMemberName] string viewName = null)
        {
            return Task.Run(() => this.ReportUsageWithCustomDataAsync(customData, viewName)).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public Task<StatisticsUpdateResponse> ReportUsageWithCustomDataAsync<T>(T customDataObject, [CallerMemberName] string viewName = null)
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

            return this.ReportUsageWithCustomDataAsync(serialized, viewName);
        }

        /// <inheritdoc />
        public StatisticsUpdateResponse ReportUsageWithCustomDataBlocking<T>(T customDataObject, [CallerMemberName] string viewName = null)
        {
            return Task.Run(() => this.ReportUsageWithCustomDataAsync(customDataObject, viewName)).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public StatisticsUpdateResponse ReportUsageBlocking([CallerMemberName] string viewName = null)
        {
            return Task.Run(() => this.ReportUsageAsync(viewName)).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public async Task<StatisticsUpdateResponse> ReportUsageWithCustomDataAsync(string customData, [CallerMemberName] string viewName = null)
        {
            StatisticsUpdateRequest request = null;
            try
            {
                await this.InitializeIfNeeded().ConfigureAwait(false);
                request = new StatisticsUpdateRequest
                {
                    ProgramId = this.LiveProgramInfo.ProgramId
                    , UserId = this.LiveProgramInfo.UserId
                    , ComponentName = viewName
                    , AssemblyVersion = this.StaticProgramInfo.PrimaryAssembly.Version
                    , FileVersion = this.StaticProgramInfo.PrimaryAssembly.FileVersion
                    //, TelemetryData = customData
                };
                string responseContent = await this.Messenger.SendPostRequest(ApiRoutes.UpdateProgramStatistics, request).ConfigureAwait(false);
                return this.Serializer.Deserialize<StatisticsUpdateResponse>(responseContent);
            }
            catch (Exception ex)
            {
                TelimenaException exception = new TelimenaException($"Error occurred while sending update [{viewName}] statistics request", ex
                    , new KeyValuePair<Type, object>(typeof(StatisticsUpdateRequest), request));
                if (!this.SuppressAllErrors)
                {
                    throw exception;
                }

                return new StatisticsUpdateResponse {Exception = exception};
            }
        }

        public async Task<StatisticsUpdateResponse> ReportEventAsync(string eventName, Dictionary<string, string> telemetryData = null)
        {
            StatisticsUpdateRequest request = null;
            try
            {
                await this.InitializeIfNeeded().ConfigureAwait(false);
                request = new StatisticsUpdateRequest
                {
                    ProgramId = this.LiveProgramInfo.ProgramId,
                    UserId = this.LiveProgramInfo.UserId,
                    ComponentName = eventName,
                    AssemblyVersion = this.StaticProgramInfo.PrimaryAssembly.Version,
                    FileVersion = this.StaticProgramInfo.PrimaryAssembly.FileVersion,
                    TelemetryData = telemetryData
                };
                string responseContent = await this.Messenger.SendPostRequest(ApiRoutes.ReportEvent, request).ConfigureAwait(false);
                return this.Serializer.Deserialize<StatisticsUpdateResponse>(responseContent);
            }
            catch (Exception ex)
            {
                TelimenaException exception = new TelimenaException($"Error occurred while sending update [{eventName}] statistics request", ex
                    , new KeyValuePair<Type, object>(typeof(StatisticsUpdateRequest), request));
                if (!this.SuppressAllErrors)
                {
                    throw exception;
                }

                return new StatisticsUpdateResponse { Exception = exception };
            }
        }

        /// <inheritdoc />
        public Task<StatisticsUpdateResponse> ReportUsageAsync([CallerMemberName] string viewName = null)
        {
            return this.ReportUsageWithCustomDataAsync(null, viewName);
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
                    ProgramInfo = this.StaticProgramInfo
                    , TelimenaVersion = this.TelimenaVersion
                    , UserInfo = this.UserInfo
                    , SkipUsageIncrementation = skipUsageIncrementation
                };
                string responseContent = await this.Messenger.SendPostRequest(ApiRoutes.RegisterClient, request).ConfigureAwait(false);
                RegistrationResponse response = this.Serializer.Deserialize<RegistrationResponse>(responseContent);
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