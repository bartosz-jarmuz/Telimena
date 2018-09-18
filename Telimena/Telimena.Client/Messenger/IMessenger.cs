using System.IO;
using System.Threading.Tasks;

namespace Telimena.Client
{
    internal interface IMessenger
    {
        Task<Stream> DownloadFile(string requestUri);
        Task<string> SendGetRequest(string requestUri);
        Task<string> SendPostRequest(string requestUri, object objectToPost);
    }
}