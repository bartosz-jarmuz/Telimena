using System;
using System.IO;
using System.Threading.Tasks;

namespace TelimenaClient
{
    /// <summary>
    ///     Telemetry and Lifecycle Management Engine App
    ///     <para>This is a client SDK that allows handling application telemetry and lifecycle</para>
    /// </summary>
    public partial class Telimena : ITelimena
    {
        /// <summary>
        ///     Gets the update request URL.
        /// </summary>
        /// <returns>System.String.</returns>
        private string GetUpdateRequestUrl(string baseUri, UpdateRequest model)
        {
            try
            {
                string stringifier = this.Serializer.Serialize(model);
                string escaped = this.Serializer.UrlEncodeJson(stringifier);
                return baseUri + "?request=" + escaped;
            }
            catch (Exception ex)
            {
                return $"Failed to get update request URL because of {ex.Message}";
            }
        }

        /// <summary>
        ///     Gets the updater update response.
        /// </summary>
        /// <returns>Task&lt;UpdateResponse&gt;.</returns>
        private async Task<UpdateResponse> GetUpdateResponse(string requestUri)
        {
            string responseContent = await this.Messenger.SendGetRequest(requestUri).ConfigureAwait(false);
            return this.Serializer.Deserialize<UpdateResponse>(responseContent);
        }

        private string GetUpdaterVersion()
        {
            FileInfo updaterFile = this.Locator.GetUpdater();
            if (updaterFile.Exists)
            {
                string version = TelimenaVersionReader.ReadToolkitVersion(updaterFile.FullName);
                return string.IsNullOrEmpty(version) ? "0.0.0.0" : version;
            }

            return "0.0.0.0";
        }
    }
}