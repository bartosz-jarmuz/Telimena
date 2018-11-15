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
        public TelemetryUpdateResponse ReportUsageWithCustomDataBlocking(string customData, [CallerMemberName] string viewName = null)
        {
            return Task.Run(() => this.ReportUsageWithCustomDataAsync(customData, viewName)).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public Task<TelemetryUpdateResponse> ReportUsageWithCustomDataAsync<T>(T customDataObject, [CallerMemberName] string viewName = null)
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
        public TelemetryUpdateResponse ReportUsageWithCustomDataBlocking<T>(T customDataObject, [CallerMemberName] string viewName = null)
        {
            return Task.Run(() => this.ReportUsageWithCustomDataAsync(customDataObject, viewName)).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public TelemetryUpdateResponse ReportUsageBlocking([CallerMemberName] string viewName = null)
        {
            return Task.Run(() => this.ReportUsageAsync(viewName)).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public async Task<TelemetryUpdateResponse> ReportUsageWithCustomDataAsync(string customData, [CallerMemberName] string viewName = null)
        {
            TelemetryUpdateRequest request = null;
            try
            {
                await this.InitializeIfNeeded().ConfigureAwait(false);
                request = new TelemetryUpdateRequest(this.TelemetryKey)
                {
                     UserId = this.LiveProgramInfo.UserId
                    , ComponentName = viewName
                    , AssemblyVersion = this.StaticProgramInfo.PrimaryAssembly.AssemblyVersion
                    , FileVersion = this.StaticProgramInfo.PrimaryAssembly.FileVersion
                    //, TelemetryData = customData
                };
                string responseContent = await this.Messenger.SendPostRequest(ApiRoutes.UpdateProgramStatistics, request).ConfigureAwait(false);
                return this.Serializer.Deserialize<TelemetryUpdateResponse>(responseContent);
            }
            catch (Exception ex)
            {
                TelimenaException exception = new TelimenaException($"Error occurred while sending update [{viewName}] statistics request", ex
                    , new KeyValuePair<Type, object>(typeof(TelemetryUpdateRequest), request));
                if (!this.SuppressAllErrors)
                {
                    throw exception;
                }

                return new TelemetryUpdateResponse {Exception = exception};
            }
        }

        public async Task<TelemetryUpdateResponse> ReportEventAsync(string eventName, Dictionary<string, string> telemetryData = null)
        {
            TelemetryUpdateRequest request = null;
            try
            {
                await this.InitializeIfNeeded().ConfigureAwait(false);
                request = new TelemetryUpdateRequest(this.TelemetryKey)
                {
                    UserId = this.LiveProgramInfo.UserId,
                    ComponentName = eventName,
                    AssemblyVersion = this.StaticProgramInfo.PrimaryAssembly.AssemblyVersion,
                    FileVersion = this.StaticProgramInfo.PrimaryAssembly.FileVersion,
                    TelemetryData = telemetryData
                };
                string responseContent = await this.Messenger.SendPostRequest(ApiRoutes.ReportEvent, request).ConfigureAwait(false);
                return this.Serializer.Deserialize<TelemetryUpdateResponse>(responseContent);
            }
            catch (Exception ex)
            {
                TelimenaException exception = new TelimenaException($"Error occurred while sending update [{eventName}] statistics request", ex
                    , new KeyValuePair<Type, object>(typeof(TelemetryUpdateRequest), request));
                if (!this.SuppressAllErrors)
                {
                    throw exception;
                }

                return new TelemetryUpdateResponse { Exception = exception };
            }
        }

        /// <inheritdoc />
        public Task<TelemetryUpdateResponse> ReportUsageAsync([CallerMemberName] string viewName = null)
        {
            return this.ReportUsageWithCustomDataAsync(null, viewName);
        }

        /// <summary>
        ///     Sends the initial app usage info
        /// </summary>
        /// <returns></returns>
        protected internal async Task<TelemetryInitializeResponse> RegisterClient(bool skipUsageIncrementation = false)
        {
            TelemetryInitializeRequest request = null;
            try
            {
                request = new TelemetryInitializeRequest(this.TelemetryKey)
                {
                    ProgramInfo = this.StaticProgramInfo
                    , TelimenaVersion = this.TelimenaVersion
                    , UserInfo = this.UserInfo
                    , SkipUsageIncrementation = skipUsageIncrementation
                };
                string responseContent = await this.Messenger.SendPostRequest(ApiRoutes.RegisterClient, request).ConfigureAwait(false);
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

                return new TelemetryInitializeResponse {Exception = exception};
            }
        }
    }
}