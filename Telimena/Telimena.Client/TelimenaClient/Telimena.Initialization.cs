﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
        internal async Task<TelemetryInitializeResponse> InitializeIfNeeded()
        {
            if (!this.IsInitialized)
            {
                TelemetryInitializeResponse response = await this.Initialize().ConfigureAwait(false);
                if (response != null && response.Exception == null)
                {
                    this.IsInitialized = true;
                    this.initializationResponse = response;
                    return this.initializationResponse;
                }
                else
                {
                    return response;
                }
            }

            return this.initializationResponse;
        }


        /// <inheritdoc />
        public async Task<TelemetryInitializeResponse> Initialize(Dictionary<string, object> telemetryData = null)
        {
            TelemetryInitializeRequest request = null;
            try
            {
                request = new TelemetryInitializeRequest(this.Properties.TelemetryKey)
                {
                    ProgramInfo = this.Properties.StaticProgramInfo
                    ,
                    TelimenaVersion = this.Properties.TelimenaVersion
                    ,
                    UserInfo = this.Properties.UserInfo
                };
                TelemetryInitializeResponse response = await this.Messenger.SendPostRequest<TelemetryInitializeResponse>(ApiRoutes.Initialize, request).ConfigureAwait(false);

                await this.LoadLiveData(response).ConfigureAwait(false);

                if (response != null && response.Exception == null)
                {
                    this.IsInitialized = true;
                    this. initializationResponse = response;
                    return this.initializationResponse;
                }
                else
                {
                    return response;
                }
            }

            catch (Exception ex)
            {
                TelimenaException exception = new TelimenaException("Error occurred while sending registration request", this.Properties, ex,
                    new KeyValuePair<Type, object>(typeof(TelemetryInitializeRequest), request));
                if (!this.Properties.SuppressAllErrors)
                {
                    throw exception;
                }

                return new TelemetryInitializeResponse { Exception = exception };
            }
        }
        internal async Task LoadLiveData(TelemetryInitializeResponse response)
        {
            try
            {
                this.propertiesInternal.LiveProgramInfo = new LiveProgramInfo(this.Properties.StaticProgramInfo) {UserId = response.UserId};

                Task<string> updaterNameTask = this.Messenger.SendGetRequest<string>($"{ApiRoutes.GetProgramUpdaterName(this.Properties.TelemetryKey)}");

                await Task.WhenAll(updaterNameTask).ConfigureAwait(false);

                this.Properties.LiveProgramInfo.UpdaterName = updaterNameTask.Result;

                if (string.IsNullOrEmpty(this.Properties.LiveProgramInfo.UpdaterName))
                {
                    throw new InvalidOperationException($"Updater name is null or empty. Task result: {updaterNameTask.Status}");
                }
            }
            catch (Exception ex)
            {
                TelimenaException exception = new TelimenaException("Error occurred while loading live program info", this.Properties, ex);
                if (!this.Properties.SuppressAllErrors)
                {
                    throw exception;
                }
            }
        }
    }
}