using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace TelimenaClient
{
    public interface IMessenger
    {
        Task<FileDownloadResult> DownloadFile(string requestUri);
        Task<HttpResponseMessage> SendGetRequest(string requestUri);
        Task<T> SendGetRequest<T>(string requestUri);
        Task<HttpResponseMessage> SendPostRequest(string requestUri, object objectToPost);
        Task<T> SendPostRequest<T>(string requestUri, object objectToPost);
    }
}