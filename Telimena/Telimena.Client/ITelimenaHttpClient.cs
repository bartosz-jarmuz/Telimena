using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TelimenaClient
{
    public interface ITelimenaHttpClient
    {
        Uri BaseUri { get; }
        Task<HttpResponseMessage> GetAsync(string requestUri);
        Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent httpContent);
    }
}