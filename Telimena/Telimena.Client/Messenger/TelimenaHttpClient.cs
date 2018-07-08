namespace Telimena.Client
{
    using System.Net.Http;
    using System.Threading.Tasks;

    internal class TelimenaHttpClient : ITelimenaHttpClient
    {
        private readonly HttpClient _client;

        public TelimenaHttpClient(HttpClient client)
        {
            this._client = client;
        }

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