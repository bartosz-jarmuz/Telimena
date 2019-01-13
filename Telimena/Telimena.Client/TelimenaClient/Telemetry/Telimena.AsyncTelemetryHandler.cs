﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelimenaClient
{
    #region Using

    #endregion

    public partial class Telimena
    {
        /// <summary>
        ///     Asynchronous Telimena methods
        /// </summary>
        internal class AsyncTelemetryHandler : IAsyncTelemetryHandler
        {
            /// <summary>
            ///     Asynchronous Telimena methods
            /// </summary>
            public AsyncTelemetryHandler(Telimena telimena, TelemetryProcessingPipeline pipeline)
            {
                this.telimena = telimena;
                this.pipeline = pipeline;
            }

            private readonly Telimena telimena;
            private readonly TelemetryProcessingPipeline pipeline;

            /// <inheritdoc />
            public async Task<TelemetryUpdateResponse> View(string viewName, Dictionary<string, object> telemetryData = null)
            {
                var unit = new TelemetryItem(viewName, TelemetryItemTypes.View, this.telimena.Properties.StaticProgramInfo.PrimaryAssembly.VersionData, telemetryData);
                await this.pipeline.Process(unit);
                return await this.Report(ApiRoutes.ReportView, viewName, telemetryData);
            }

            /// <inheritdoc />
            public Task<TelemetryUpdateResponse> Event(string eventName, Dictionary<string, object> telemetryData = null)
            {
                return this.Report(ApiRoutes.ReportEvent, eventName, telemetryData);
            }

            /// <inheritdoc />
            public async Task<TelemetryInitializeResponse> Initialize(Dictionary<string, object> telemetryData = null)
            {
                TelemetryInitializeRequest request = null;
                try
                {
                    request = new TelemetryInitializeRequest(this.telimena.Properties.TelemetryKey)
                    {
                        ProgramInfo = this.telimena.Properties.StaticProgramInfo
                        , TelimenaVersion = this.telimena.Properties.TelimenaVersion
                        , UserInfo = this.telimena.Properties.UserInfo
                        , SkipUsageIncrementation = false
                    };
                    string responseContent = await this.telimena.Messenger.SendPostRequest(ApiRoutes.Initialize, request).ConfigureAwait(false);
                    TelemetryInitializeResponse response = this.telimena.Serializer.Deserialize<TelemetryInitializeResponse>(responseContent);

                    await this.telimena.LoadLiveData(response).ConfigureAwait(false);



                    return response;
                }

                catch (Exception ex)
                {
                    TelimenaException exception = new TelimenaException("Error occurred while sending registration request", this.telimena.Properties, ex
                        , new KeyValuePair<Type, object>(typeof(TelemetryUpdateRequest), request));
                    if (!this.telimena.Properties.SuppressAllErrors)
                    {
                        throw exception;
                    }

                    return new TelemetryInitializeResponse {Exception = exception};
                }
            }

            /// <summary>
            ///     Where the telemetry begins...
            /// </summary>
            /// <param name="apiRoute"></param>
            /// <param name="componentName"></param>
            /// <param name="telemetryData"></param>
            /// <returns></returns>
            private async Task<TelemetryUpdateResponse> Report(string apiRoute, string componentName, Dictionary<string, object> telemetryData = null)
            {
                TelemetryUpdateRequest request = null;
                try
                {
                    TelemetryInitializeResponse result = await this.telimena.InitializeIfNeeded().ConfigureAwait(false);
                    if (result.Exception != null)
                    {
                        throw result.Exception;
                    }

                    request = new TelemetryUpdateRequest(this.telimena.Properties.TelemetryKey)
                    {
                        DebugMode = this.telimena.Properties.GetFullApiResponse,
                        UserId = this.telimena.Properties.LiveProgramInfo.UserId,
                        //ComponentName = componentName,
                        //VersionData = this.telimena.Properties.StaticProgramInfo.PrimaryAssembly.VersionData,
                        //TelemetryData = telemetryData?.ToDictionary(x=>x.Key, y=>y.Value.ToString())
                    };
                    string responseContent = await this.telimena.Messenger.SendPostRequest(apiRoute, request).ConfigureAwait(false);
                    return this.telimena.Serializer.Deserialize<TelemetryUpdateResponse>(responseContent);
                }
                catch (Exception ex)
                {
                    TelimenaException exception = new TelimenaException($"Error occurred while sending update [{componentName}] telemetry request to [{apiRoute}]", this.telimena.Properties, ex
                        , new KeyValuePair<Type, object>(typeof(TelemetryUpdateRequest), request));
                    if (!this.telimena.Properties.SuppressAllErrors)
                    {
                        throw exception;
                    }

                    return new TelemetryUpdateResponse { Exception = exception };
                }
            }
        }
    }
}