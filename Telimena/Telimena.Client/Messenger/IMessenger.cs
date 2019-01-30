using System.Threading.Tasks;
using TelimenaClient.Model.Internal;

namespace TelimenaClient
{
    internal interface IMessenger
    {
        Task<FileDownloadResult> DownloadFile(string requestUri);
        Task<T> SendGetRequest<T>(string requestUri);
        Task<T> SendPostRequest<T>(string requestUri, object objectToPost);
    }
}