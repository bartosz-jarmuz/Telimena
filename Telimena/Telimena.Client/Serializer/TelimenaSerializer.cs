using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Script.Serialization;

namespace TelimenaClient.Serializer
{
    /// <summary>
    /// Handles serialization 
    /// </summary>
    public class TelimenaSerializer : ITelimenaSerializer
    {

        private static DataContractJsonSerializer GetDataContractJsonSerializer(Type type)
        {
            DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings()
            {
                DateTimeFormat = new DateTimeFormat("O")
                , UseSimpleDictionaryFormat = true
                , DataContractSurrogate = new DateTimeDataContractSurrogate()
                , KnownTypes = new Type[]
                {
                    typeof(DateTimeOffset),
                    typeof(DateTime),
                }
            };

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(type, settings);
            return serializer;
        }


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