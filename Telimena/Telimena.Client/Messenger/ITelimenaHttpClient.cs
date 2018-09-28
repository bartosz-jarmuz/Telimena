using System.Net.Http;
using System.Threading.Tasks;

namespace Telimena.ToolkitClient
{
    internal interface ITelimenaHttpClient
    {
        Task<HttpResponseMessage> GetAsync(string requestUri);
        Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent httpContent);
    }
}