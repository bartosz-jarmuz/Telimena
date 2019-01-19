using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace TelimenaClient
{
    internal class TelemetryRequestSender
    {

        private readonly Telimena telimena;

        public TelemetryRequestSender(Telimena telimena)
        {
            this.telimena = telimena;
        }

        public async Task<TelemetryUpdateResponse> SendRequests(TelemetryUpdateRequest request, List<FileInfo> associatedFiles)
        {
            try
            {
                TelemetryUpdateResponse result = await this.Send(request).ConfigureAwait(false);

                if (result.StatusCode == HttpStatusCode.Accepted)
                {
                    foreach (var associatedFile in associatedFiles)
                    {
                        try
                        {
                            await Retrier.RetryAsync(() => { File.Delete(associatedFile.FullName); }
                                , TimeSpan.FromMilliseconds(250), 3).ConfigureAwait(false);
                        }
                        catch
                        {
                            //if it fails, it fails
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                TelimenaException exception = new TelimenaException($"Error occurred while sending update telemetry request to [{ApiRoutes.PostTelemetryData}]", this.telimena.Properties, ex
                    , new KeyValuePair<Type, object>(typeof(TelemetryUpdateRequest), request));
                if (!this.telimena.Properties.SuppressAllErrors)
                {
                    throw exception;
                }

                return new TelemetryUpdateResponse(exception);
            }
        }

        private async Task<TelemetryUpdateResponse> Send(TelemetryUpdateRequest request)
        {
            TelemetryInitializeResponse result = await this.telimena.InitializeIfNeeded().ConfigureAwait(false);
            if (result.Exception != null)
            {
                throw result.Exception;
            }

            HttpResponseMessage response = await this.telimena.Messenger.SendPostRequest(ApiRoutes.PostTelemetryData, request).ConfigureAwait(false);
            return new TelemetryUpdateResponse(response);
        }
    }
}