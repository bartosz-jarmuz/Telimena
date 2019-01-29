
using System.Collections.Generic;
using Telimena.WebApp.Core.DTO.AppInsightsTelemetryModel;
using TelimenaClient;

namespace Telimena.WebApp.Utils
{
    /// <summary>
    /// Class AppInsightsTelemetryAdapter.
    /// </summary>
    public static class AppInsightsTelemetryAdapter
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
            TelemetryItemTypes type = GetItemType(appInsightsTelemetry);
            var props = appInsightsTelemetry.data.baseData.properties;
            var versionData = new VersionData(props.ProgramAssemblyVersion,props.ProgramFileVersion);
            var item = new TelemetryItem(props.TelemetryKey, type, versionData,null);
            return item;
        }


        /// <summary>
        /// Gets the type of the item.
        /// </summary>
        /// <param name="telemetry">The dynamic object.</param>
        /// <returns>TelemetryItemTypes.</returns>
        private static TelemetryItemTypes GetItemType(AppInsightsTelemetry telemetry)
        {

            string baseType = telemetry?.data?.baseType;
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