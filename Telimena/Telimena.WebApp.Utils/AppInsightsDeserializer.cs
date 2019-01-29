using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Telimena.WebApp.Core.DTO.AppInsightsTelemetryModel;
using JsonSerializer = Microsoft.ApplicationInsights.Extensibility.Implementation.JsonSerializer;

namespace Telimena.WebApp.Utils
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
        public static IEnumerable<AppInsightsTelemetry> Deserialize(byte[] telemetryItemsData, bool compress = true)
        {
            string deserialized = JsonSerializer.Deserialize(telemetryItemsData, compress);
            if (!string.IsNullOrEmpty(deserialized))
            {
                string[] items = deserialized.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in items)
                {
                    yield return JsonConvert.DeserializeObject<AppInsightsTelemetry>(item);
                }
            }
        }
    }
}