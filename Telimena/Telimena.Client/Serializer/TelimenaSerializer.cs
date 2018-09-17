using System;

namespace Telimena.Client
{
    using System.Web.Script.Serialization;

    internal class TelimenaSerializer : ITelimenaSerializer
    {

        public string Serialize(object objectToPost)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new JavaScriptConverter[] { new NullPropertiesConverter() });
            string jsonObject = serializer.Serialize(objectToPost);
            return jsonObject;
        }

        string ITelimenaSerializer.UrlEncodeJson(string jsonString)
        {
            return TelimenaSerializer.UrlEncodeJson(jsonString);
        }

        string ITelimenaSerializer.UrlDecodeJson(string escapedJsonString)
        {
            return TelimenaSerializer.UrlDecodeJson(escapedJsonString);
        }

        public T Deserialize<T>(string stringContent)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Deserialize<T>(stringContent);
        }

        public static string UrlEncodeJson(string jsonString)
        {
            return Uri.EscapeDataString(jsonString);
        }

        public static string UrlDecodeJson(string escapedJsonString)
        {
            return Uri.UnescapeDataString(escapedJsonString);
        }

    }
}