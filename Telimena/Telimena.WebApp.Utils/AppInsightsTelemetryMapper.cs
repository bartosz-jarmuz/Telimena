using System.Collections.Generic;
using System.Linq;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.DTO.AppInsightsTelemetryModel;
using Telimena.WebApp.Core.DTO.MappableToClient;

namespace Telimena.WebApp.Utils
{
    /// <summary>
    /// Class AppInsightsTelemetryMapper.
    /// </summary>
    public static class AppInsightsTelemetryMapper
    {
        /// <summary>
        /// Maps the specified items.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <returns>List&lt;TelemetryItem&gt;.</returns>
        public static IEnumerable<TelemetryItem> Map(IEnumerable<AppInsightsTelemetry> items)
        {
            foreach (AppInsightsTelemetry appInsightsTelemetry in items)
            {
                yield return Build(appInsightsTelemetry);
            }
        }

        private static TelemetryItem Build(AppInsightsTelemetry appInsightsTelemetry)
        {
            Dictionary<string, string> appInsightsProperties = appInsightsTelemetry.Data.BaseData.Properties;

            TelemetryItem item = new TelemetryItem()
            {
                Timestamp = appInsightsTelemetry.Time,
                VersionData = GetVersionData(appInsightsProperties),
                UserId = appInsightsTelemetry.Tags.AiUserId,
                TelemetryData = GetFilteredProperties(appInsightsProperties),
                TelemetryItemType = GetItemType(appInsightsTelemetry),
                EntryKey = appInsightsTelemetry.Data.BaseData.Name,
                Sequence = appInsightsTelemetry.Seq
            };

            return item;
        }

        private static Dictionary<string, string> GetFilteredProperties(Dictionary<string, string> properties)
        {
            var propNames = typeof(TelimenaContextPropertyKeys).GetProperties().Select(x => x.Name).ToList();
            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> keyValuePair in properties)
            {
                if (!propNames.Contains(keyValuePair.Key))
                {
                    result.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }

            return result;
        }

        private static VersionData GetVersionData(Dictionary<string, string> properties)
        {
            if (!properties.TryGetValue(TelimenaContextPropertyKeys.ProgramFileVersion, out string file))
            {
                file = "0.0.0.0";
            }

            if (!properties.TryGetValue(TelimenaContextPropertyKeys.ProgramAssemblyVersion, out string ass))
            {
                ass = "0.0.0.0";
            }



            return new VersionData(ass, file);

        }

        /// <summary>
        /// Gets the type of the item.
        /// </summary>
        /// <param name="telemetry">The dynamic object.</param>
        /// <returns>TelemetryItemTypes.</returns>
        private static TelemetryItemTypes GetItemType(AppInsightsTelemetry telemetry)
        {

            string baseType = telemetry?.Data?.BaseType;
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