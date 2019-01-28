using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TelimenaClient;
using JsonSerializer = Microsoft.ApplicationInsights.Extensibility.Implementation.JsonSerializer;

namespace Telimena.WebApp.Controllers.Api.V1
{
    /// <summary>
    /// Class AppInsightsDeserializer.
    /// </summary>
    public static class AppInsightsDeserializer
    {


        /// <summary>
        /// Deserializes and decompress the telemetry items into a collection.
        /// </summary>
        /// <param name="telemetryItemsData">Serialized telemetry items.</param>
        /// <param name="compress">Should deserialization also perform decompression.</param>
        /// <returns>Telemetry items serialized as a string.</returns>
        public static IEnumerable<dynamic> Deserialize(byte[] telemetryItemsData, bool compress = true)
        {
            string deserialized = JsonSerializer.Deserialize(telemetryItemsData, compress);
            if (!string.IsNullOrEmpty(deserialized))
            {
                string[] items = deserialized.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in items)
                {
                    dynamic dynamicObject = JsonConvert.DeserializeObject(item);

                    yield return dynamicObject;
                }
            }
        }

        public static IEnumerable<TelemetryItem> BuildItems(IEnumerable<dynamic> dynamicObjects)
        {
            foreach (var dynamicObject in dynamicObjects)
            {
                var itemType = GetItemType(dynamicObject);

                var item = new TelemetryItem(Guid.NewGuid().ToString(), itemType, null, null);
                yield return item;
            }
        }

        private static TelemetryItemTypes GetItemType(dynamic dynamicObject)
        {

            string baseType = dynamicObject?.data?.baseType;
            switch (baseType)
            {
                case "PageViewData":
                    return TelemetryItemTypes.View;
                default:
                    return TelemetryItemTypes.Event;
            }

        }
    }
}