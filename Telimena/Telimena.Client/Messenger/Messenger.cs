namespace Telimena.Client
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    internal class Messenger : IMessenger
    {
        public ITelimenaSerializer Serializer { get; }
        public ITelimenaHttpClient HttpClient { get; }
        public bool SuppressAllErrors { get; }

        public Messenger(ITelimenaSerializer serializer, ITelimenaHttpClient httpClient, bool suppressAllErrors)
        {
            this.Serializer = serializer;
            this.HttpClient = httpClient;
            this.SuppressAllErrors = suppressAllErrors;
        }
        public async Task<string> SendPostRequest(string requestUri, object objectToPost)
        {
            try
            {
                string jsonObject = this.Serializer.Serialize(objectToPost);
                StringContent content = new StringContent(jsonObject, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await this.HttpClient.PostAsync(requestUri, content);
                string responseContent = await response.Content.ReadAsStringAsync();
                return responseContent;
            }
            catch (Exception)
            {
                if (!this.SuppressAllErrors)
                {
                    throw;
                }

                return "";
            }
        }

        public async Task<string> SendGetRequest(string requestUri)
        {
            try
            {
                HttpResponseMessage response = await this.HttpClient.GetAsync(requestUri);
                string responseContent = await response.Content.ReadAsStringAsync();
                return responseContent;
            }
            catch (Exception)
            {
                if (!this.SuppressAllErrors)
                {
                    throw;
                }

                return "";
            }
        }

    }
}