using System;
using System.Web.Script.Serialization;

namespace TelimenaClient.Serializer
{
    /// <summary>
    /// Handles serialization 
    /// </summary>
    public class TelimenaSerializer : ITelimenaSerializer
    {
        /// <summary>
        /// Converts object to string
        /// </summary>
        /// <param name="objectToPost"></param>
        /// <returns></returns>
        public string Serialize(object objectToPost)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new JavaScriptConverter[] {new NullPropertiesConverter()});
            string jsonObject = serializer.Serialize(objectToPost);
            return jsonObject;
        }

        string ITelimenaSerializer.UrlEncodeJson(string jsonString)
        {
            return UrlEncodeJson(jsonString);
        }

        string ITelimenaSerializer.UrlDecodeJson(string escapedJsonString)
        {
            return UrlDecodeJson(escapedJsonString);
        }

        /// <summary>
        /// Object from string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stringContent"></param>
        /// <returns></returns>
        public T Deserialize<T>(string stringContent)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Deserialize<T>(stringContent);
        }

        /// <summary>
        /// Converts to string and URL encodes (so that Json can be sent as GET)
        /// </summary>
        /// <param name="objectToPost"></param>
        /// <returns></returns>
        public string SerializeAndEncode(object objectToPost)
        {
            string json = this.Serialize(objectToPost);
            return UrlEncodeJson(json);
        }

        /// <summary>
        /// Converts url encoded string to JSON
        /// </summary>
        /// <param name="escapedJsonString"></param>
        /// <returns></returns>
        public static string UrlDecodeJson(string escapedJsonString)
        {
            return Uri.UnescapeDataString(escapedJsonString);
        }

        /// <summary>
        /// URL Encodes json string
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static string UrlEncodeJson(string jsonString)
        {
            return Uri.EscapeDataString(jsonString);
        }
    }
}