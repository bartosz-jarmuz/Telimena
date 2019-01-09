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

        /// <summary>
        /// Converts object to string
        /// </summary>
        /// <param name="objectToPost"></param>
        /// <returns></returns>
        private string SerializeTelemetryItem(TelemetryItem objectToPost)
        {
            using (MemoryStream stream = new System.IO.MemoryStream())
            {
                DataContractJsonSerializer serializer = GetDataContractJsonSerializer(objectToPost.GetType());
                serializer.WriteObject(stream, objectToPost);
                stream.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

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

        /// <summary>
        /// Object from string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stringContent"></param>
        /// <returns></returns>
        private object DeserializeTelemetryItem(string stringContent)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(stringContent)))
                {
                    DataContractJsonSerializer serializer = GetDataContractJsonSerializer(typeof(TelemetryItem));
                    return (TelemetryItem)serializer.ReadObject(ms);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error while deserializing string as {typeof(TelemetryItem).Name}", ex); 
            }
        }

        /// <inheritdoc />
        public T Deserialize<T>(string stringContent)
        {
            if (typeof(T) == typeof(TelemetryItem))
            {
                return (T) this.DeserializeTelemetryItem(stringContent);
            }
            else
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
        }

        /// <inheritdoc />
        public string Serialize(object objectToPost)
        {
            if (objectToPost is TelemetryItem telemetryItem)
            {
                return this.SerializeTelemetryItem(telemetryItem);
            }
            else
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializer.RegisterConverters(new JavaScriptConverter[] { new PropertiesConverter() });
                string jsonObject = serializer.Serialize(objectToPost);
                return jsonObject;
            }
        }
    }
}