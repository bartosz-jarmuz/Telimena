using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Telimena.Client
{
    internal class TelimenaHttpClient : ITelimenaHttpClient
    {
        public TelimenaHttpClient(HttpClient client)
        {
            this.client = client;
        }

        private readonly HttpClient client; 

        #region Implementation of IHttpClient

        public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent httpContent)
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            return this.client.PostAsync(requestUri, httpContent);
        }

        public Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            return this.client.GetAsync(requestUri);
        }

        #endregion
    }
}