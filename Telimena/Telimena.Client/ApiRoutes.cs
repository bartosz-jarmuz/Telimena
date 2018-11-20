namespace TelimenaClient
{
    internal static class ApiRoutes
    {
        public static string UpdateProgramStatistics => "api/Statistics/Update";
        public static string ReportEvent => "api/Telemetry/Event";
        public static string ReportView => "api/Telemetry/View";
        public static string Initialize => "api/Telemetry/Initialize";
        public static string GetProgramUpdateInfo => "api/ProgramUpdates/GetUpdateInfo";
        public static string GetUpdaterUpdateInfo => "api/Updater/GetUpdateInfo";
        public static string GetProgramUpdaterName => "api/Updater/GetProgramUpdaterName";

        public static class QueryParams
        {
            public static string TelemetryKey => "telemetryKey";

        }
    }
}