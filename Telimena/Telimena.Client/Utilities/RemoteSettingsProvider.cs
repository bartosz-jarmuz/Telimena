using Microsoft.ApplicationInsights.Extensibility.Implementation;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TelimenaClient
{
    internal class RemoteSettingsProvider : IRemoteSettingsProvider
    {
        private readonly Uri baseUrl;

        public RemoteSettingsProvider(Uri baseUrl)
        {
            this.baseUrl = baseUrl;
        }

        public async Task<string> GetUserTrackingSettings(Guid telemetryKey)
        {
            string stringified = null;
            try
            {
                using (HttpClient client = new HttpClient() { BaseAddress = this.baseUrl })
                {
                    HttpResponseMessage response = await client.GetAsync(ApiRoutes.GetTelemetrySettings(telemetryKey));
                    stringified = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                    return stringified;
                }
            }
            catch (Exception ex)
            {
                TelemetryDebugWriter.WriteError($"Error while loading instrumentation key. Error: {ex}. Response: {stringified}");
                return null;
            }
        }
    }
}