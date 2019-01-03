using System;

namespace TelimenaClient
{
    internal static class ApiRoutes
    {
        public static string ReportEvent => "api/v1/Telemetry/Event";
        public static string ReportView => "api/v1/Telemetry/View";
        public static string Initialize => "api/v1/Telemetry/Initialize";
        public static string ProgramUpdateCheck => $"api/v1/programs/update-check";
        public static string UpdaterUpdateCheck => $"api/v1/updater/update-check/";
        public static string GetProgramUpdaterName(Guid telemetryKey) => $"api/v1/programs/{telemetryKey}/updater/name";

       
    }
}