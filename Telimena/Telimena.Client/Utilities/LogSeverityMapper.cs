using System;
using Microsoft.ApplicationInsights.DataContracts;
using TelimenaClient.Model;

namespace TelimenaClient
{
    internal static class LogSeverityMapper
    {
        public static SeverityLevel Map(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    return SeverityLevel.Verbose;
                case LogLevel.Info:
                    return SeverityLevel.Information;
                case LogLevel.Warn:
                    return SeverityLevel.Warning;
                case LogLevel.Error:
                    return SeverityLevel.Error;
                case LogLevel.Critical:
                    return SeverityLevel.Critical;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }
    }
}