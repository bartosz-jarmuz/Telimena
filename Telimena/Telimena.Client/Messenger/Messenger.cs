using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TelimenaClient.Model.Internal;
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

        public async Task<T> SendGetRequest<T>(string requestUri)
        {
            HttpResponseMessage content = await this.SendGetRequest(requestUri).ConfigureAwait(false);
            try
            {
                string stringified = await content.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (typeof(T) == typeof(string))
                {
                    return (T)(object)stringified;
                }
                return this.Serializer.Deserialize<T>(stringified);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"An error occurred while deserializing response from GET to [{requestUri}]. Base URL [{this.HttpClient.BaseUri}]", e);
            }
        }

        public async Task<HttpResponseMessage> SendPostRequest(string requestUri, object objectToPost)
        {
            try
            {
                string jsonObject = this.Serializer.Serialize(objectToPost);
                StringContent content = new StringContent(jsonObject, Encoding.UTF8, "application/json");
                List<TimeSpan> retryIntervals = new List<TimeSpan>() { TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(250), TimeSpan.FromMilliseconds(500) };

                return await Retrier.RetryTaskAsync(() => this.HttpClient.PostAsync(requestUri, content), (Exception ex) => ex.GetType() == typeof(SocketException), retryIntervals).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
               throw new InvalidOperationException($"An error occurred while posting to [{requestUri}]. Base URL [{this.HttpClient.BaseUri}]",ex);
            }
        }

        public async Task<T> SendPostRequest<T>(string requestUri, object objectToPost)
        {
            HttpResponseMessage content = await this.SendPostRequest(requestUri, objectToPost).ConfigureAwait(false);
            try
            {
                string stringified = await content.Content.ReadAsStringAsync().ConfigureAwait(false);
                return this.Serializer.Deserialize<T>(stringified);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("An error occurred while deserializing response from POST to [{requestUri}]. Base URL [{this.HttpClient.BaseUri}]", e);
            }
        }

        public Task<HttpResponseMessage> SendGetRequest(string requestUri)
        {
            try
            {
                return this.HttpClient.GetAsync(requestUri);
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

                Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
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