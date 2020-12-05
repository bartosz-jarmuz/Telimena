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
            using (HttpClient client = new HttpClient() { BaseAddress = this.baseUrl })
            {
                HttpResponseMessage response = await client.GetAsync(ApiRoutes.GetTelemetrySettings(telemetryKey));
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}