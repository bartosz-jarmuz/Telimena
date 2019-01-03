using System;
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
                TelemetryInitializeResponse response = await this.telemetry.Async.Initialize().ConfigureAwait(false);
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

        private async Task LoadLiveData(TelemetryInitializeResponse response)
        {
            try
            {
                this.properties.LiveProgramInfo = new LiveProgramInfo(this.Properties.StaticProgramInfo) {UserId = response.UserId};

                Task<string> updaterNameTask =
                    this.Messenger.SendGetRequest($"{ApiRoutes.GetProgramUpdaterName(this.Properties.TelemetryKey)}");

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