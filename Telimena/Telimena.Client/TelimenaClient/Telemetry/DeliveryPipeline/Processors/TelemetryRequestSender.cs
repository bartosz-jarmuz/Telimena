using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace TelimenaClient
{
    internal class TelemetryRequestSender : ITelemetryBroadcastingProcessor
    {
        private readonly Telimena telimena;

        public TelemetryRequestSender(Telimena telimena)
        {
            this.telimena = telimena;
        }

        public async Task Process(TelemetryBroadcastingContext context)
        {
            try
            {
                var response = await this.Send(context.Data.Select(x=>x.SerializedData)).ConfigureAwait(false);
                context.Response = response;
            }
            catch (Exception ex)
            {
                TelimenaException exception = new TelimenaException($"Error occurred while sending update telemetry request to [{ApiRoutes.PostTelemetryData}]", this.telimena.Properties, ex);
                if (!this.telimena.Properties.SuppressAllErrors)
                {
                    throw exception;
                }

                context.Response = new TelemetryUpdateResponse(exception);
            }
        }

        private async Task<TelemetryUpdateResponse> Send(IEnumerable<string> serializedTelemetryItems)
        {
            TelemetryInitializeResponse result = await this.telimena.InitializeIfNeeded().ConfigureAwait(false);
            if (result.Exception != null)
            {
                throw result.Exception;
            }

            var request = new TelemetryUpdateRequest(this.telimena.Properties.TelemetryKey, this.telimena.Properties.LiveProgramInfo.UserId, serializedTelemetryItems.ToList());

            HttpResponseMessage response = await this.telimena.Messenger.SendPostRequest(ApiRoutes.PostTelemetryData, request).ConfigureAwait(false);
            return new TelemetryUpdateResponse(response);
        }
    }
}