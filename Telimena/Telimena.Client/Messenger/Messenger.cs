using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TelimenaClient.Serializer;

namespace TelimenaClient
{
    internal class Messenger : IMessenger
    {
        public Messenger(ITelimenaSerializer serializer, ITelimenaHttpClient httpClient)
        {
            this.Serializer = serializer;
            this.HttpClient = httpClient;
        }

        public ITelimenaSerializer Serializer { get; }
        public ITelimenaHttpClient HttpClient { get; }

        public async Task<string> SendPostRequest(string requestUri, object objectToPost)
        {
            try
            {
                string jsonObject = this.Serializer.Serialize(objectToPost);
                StringContent content = new StringContent(jsonObject, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await this.HttpClient.PostAsync(requestUri, content).ConfigureAwait(false);

                var retryIntervals = new List<TimeSpan>() {TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(250), TimeSpan.FromMilliseconds(500)};
                var retryExceptionFilters = new Retry.ExceptionFilterSet(new List<Retry.ExceptionFilter>()
                {
                    new Retry.ExceptionFilter(typeof(SocketException))
                }, null);


                string responseContent = await Retry.DoAsync( ()=> response.Content.ReadAsStringAsync(), retryIntervals, retryExceptionFilters);
                return responseContent; 
            }
            catch (Exception ex)
            {
               throw new InvalidOperationException($"An error occured while posting to [{requestUri}]",ex);
            }
        }

        public async Task<string> SendGetRequest(string requestUri)
        {
            try
            {
                HttpResponseMessage response = await this.HttpClient.GetAsync(requestUri).ConfigureAwait(false);
                string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return responseContent;
            }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"An error occured while getting from [{requestUri}]", ex);
                }
        }

        public async Task<FileDownloadResult> DownloadFile(string requestUri)
        {
            try
            {
                HttpResponseMessage response = await this.HttpClient.GetAsync(requestUri).ConfigureAwait(false);

                var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                return new FileDownloadResult()
                {
                    FileName = response.Content.Headers.ContentDisposition.FileName.Trim('\"')
                    ,Stream = stream
                };
            }
        
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occured while downloading from [{requestUri}]", ex);
            }
        }
    }
}