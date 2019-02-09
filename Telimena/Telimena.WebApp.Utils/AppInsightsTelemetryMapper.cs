using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using DotNetLittleHelpers;
using Microsoft.ApplicationInsights.DataContracts;
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
                Sequence = appInsightsTelemetry.Seq,
                LogMessage = appInsightsTelemetry.Data.BaseData.Message
                
            };
            MapLogLevel(appInsightsTelemetry, item);

            MapExceptionProperties(appInsightsTelemetry, item);

            return item;
        }


        private static void MapLogLevel(AppInsightsTelemetry appInsightsTelemetry, TelemetryItem item)
        {
            if (appInsightsTelemetry.Data.BaseData.SeverityLevel != null)
            {
                if (Enum.TryParse(appInsightsTelemetry.Data.BaseData.SeverityLevel, out SeverityLevel enumerized))
                {
                    item.LogLevel = Map(enumerized);
                }
            }
        }

        private static void MapExceptionProperties(AppInsightsTelemetry appInsightsTelemetry, TelemetryItem item)
        {
            if (!appInsightsTelemetry.Data.BaseData.Exceptions.IsNullOrEmpty())
            {
                item.Exceptions = new List<TelemetryItem.ExceptionInfo>();
                foreach (ExceptionElement baseDataException in appInsightsTelemetry.Data.BaseData.Exceptions)
                {
                    item.Exceptions.Add(Mapper.Map<TelemetryItem.ExceptionInfo>(baseDataException));
                }
            }
        }

        private static LogLevel Map(SeverityLevel level)
        {
            switch (level)
            {
                case SeverityLevel.Verbose:
                    return LogLevel.Debug;
                case SeverityLevel.Information:
                    return LogLevel.Info;
                case SeverityLevel.Warning:
                    return LogLevel.Warn;
                case SeverityLevel.Error:
                return LogLevel.Error;
                case SeverityLevel.Critical:
                    return LogLevel.Critical;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
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
                case "ExceptionData":
                    return TelemetryItemTypes.Exception;
                case "MessageData":
                    return TelemetryItemTypes.LogMessage;
                default:
                    return TelemetryItemTypes.Event;
            }

        }
    }
}