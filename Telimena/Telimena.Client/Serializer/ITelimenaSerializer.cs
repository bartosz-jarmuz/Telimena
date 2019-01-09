
namespace TelimenaClient.Serializer
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
        /// Converts object to string
        /// </summary>
        /// <param name="objectToPost"></param>
        /// <returns></returns>
        string SerializeTelemetryItem(TelemetryItem objectToPost);

        /// <summary>
        /// Object from string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stringContent"></param>
        /// <returns></returns>
        TelemetryItem DeserializeTelemetryItem(string stringContent);
    }
}