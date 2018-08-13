using System.IO;

namespace Telimena.Client
{
    using System.Threading.Tasks;

    internal interface IMessenger
    {
        Task<string> SendPostRequest(string requestUri, object objectToPost);
        Task<string> SendGetRequest(string requestUri);
        Task<Stream> DownloadFile(string requestUri);
    }
}