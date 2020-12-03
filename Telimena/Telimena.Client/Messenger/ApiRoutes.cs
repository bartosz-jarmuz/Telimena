using System;

namespace TelimenaClient
{
    internal static class ApiRoutes
    {
        public static string PostTelemetryData => "api/v1/telemetry";
        public static string Initialize => "api/v1/telemetry/initialize";
        public static string ProgramUpdateCheck => $"api/v1/programs/update-check";
        public static string UpdaterUpdateCheck => $"api/v1/updaters/update-check";
        public static string GetProgramUpdaterName(Guid telemetryKey) => $"api/v1/programs/{telemetryKey}/updater/name";
        public static string GetInstrumentationKey(Guid telemetryKey) => $"api/v1/programs/{telemetryKey}/instrumentation-key";
        public static string GetTelemetrySettings(Guid telemetryKey) => $"api/v1/programs/{telemetryKey}/telemetry-settings";

       
    }
}