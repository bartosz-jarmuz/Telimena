using System;
using System.Web.Script.Serialization;

namespace TelimenaClient.Serializer
{
    /// <summary>
    /// Handles serialization 
    /// </summary>
    public class TelimenaSerializer : ITelimenaSerializer
    {

        /// <inheritdoc />
        public T Deserialize<T>(string stringContent)
        {
                try
                {
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    return serializer.Deserialize<T>(stringContent);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error while deserializing string as {typeof(T).Name}", ex);
                }
        }

        /// <inheritdoc />
        public string Serialize(object objectToPost)
        {

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializer.RegisterConverters(new JavaScriptConverter[] { new PropertiesConverter() });
                string jsonObject = serializer.Serialize(objectToPost);
                return jsonObject;
        }
    }
}