using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using TelimenaClient.Model;
using TelimenaClient.Model.Internal;

namespace TelimenaClient
{
    internal interface IMessenger
    {
        Task<FileDownloadResult> DownloadFile(string requestUri);
        Task<HttpResponseMessage> SendGetRequest(string requestUri);
        Task<T> SendGetRequest<T>(string requestUri);
        Task<HttpResponseMessage> SendPostRequest(string requestUri, object objectToPost);
        Task<T> SendPostRequest<T>(string requestUri, object objectToPost);
    }
}