using System;
using System.Web.Script.Serialization;

namespace Telimena.Client
{
    public class TelimenaSerializer : ITelimenaSerializer
    {
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

        public T Deserialize<T>(string stringContent)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Deserialize<T>(stringContent);
        }

        public string SerializeAndEncode(object objectToPost)
        {
            string json = this.Serialize(objectToPost);
            return UrlEncodeJson(json);
        }

        public static string UrlDecodeJson(string escapedJsonString)
        {
            return Uri.UnescapeDataString(escapedJsonString);
        }

        public static string UrlEncodeJson(string jsonString)
        {
            return Uri.EscapeDataString(jsonString);
        }
    }
}