using System.Net.Http;
using System.Threading.Tasks;

namespace Telimena.Client
{
    internal class TelimenaHttpClient : ITelimenaHttpClient
    {
        public TelimenaHttpClient(HttpClient client)
        {
            this._client = client;
        }

        private readonly HttpClient _client;

        #region Implementation of IHttpClient

        public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent httpContent)
        {
            return this._client.PostAsync(requestUri, httpContent);
        }

        public Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            return this._client.GetAsync(requestUri);
        }

        #endregion
    }
}