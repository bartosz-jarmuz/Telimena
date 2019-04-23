using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TelimenaClient
{
    internal interface ITelimenaHttpClient
    {
        Uri BaseUri { get; }
        Task<HttpResponseMessage> GetAsync(string requestUri);
        Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent httpContent);
    }
}