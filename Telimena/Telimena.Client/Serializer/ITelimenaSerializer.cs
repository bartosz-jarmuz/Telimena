
namespace Telimena.ToolkitClient.Serializer
{
    /// <summary>
    /// Interface ITelimenaSerializer
    /// </summary>
    public interface ITelimenaSerializer
    {
        /// <summary>
        /// Deserializes the specified string content.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stringContent">Content of the string.</param>
        /// <returns>T.</returns>
        T Deserialize<T>(string stringContent);
        /// <summary>
        /// Serializes the specified object to post.
        /// </summary>
        /// <param name="objectToPost">The object to post.</param>
        /// <returns>System.String.</returns>
        string Serialize(object objectToPost);
        /// <summary>
        /// Serializes the and encode.
        /// </summary>
        /// <param name="objectToPost">The object to post.</param>
        /// <returns>System.String.</returns>
        string SerializeAndEncode(object objectToPost);
        /// <summary>
        /// URLs the decode json.
        /// </summary>
        /// <param name="escapedJsonString">The escaped json string.</param>
        /// <returns>System.String.</returns>
        string UrlDecodeJson(string escapedJsonString);
        /// <summary>
        /// URLs the encode json.
        /// </summary>
        /// <param name="jsonString">The json string.</param>
        /// <returns>System.String.</returns>
        string UrlEncodeJson(string jsonString);
    }
}