namespace Telimena.Client
{
    using System.Net.Http;
    using System.Threading.Tasks;

    internal interface ITelimenaHttpClient
    {
        Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent httpContent);
        Task<HttpResponseMessage> GetAsync(string requestUri);
    }
}