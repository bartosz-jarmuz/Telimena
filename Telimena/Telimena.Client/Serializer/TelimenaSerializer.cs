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

        public T Deserialize<T>(string stringContent)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Deserialize<T>(stringContent);
        }

    }
}