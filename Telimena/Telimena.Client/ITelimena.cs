namespace Telimena.Client
{
    using System.Threading.Tasks;

    /// <summary>
    /// Telemetry and Lifecycle Management Engine App
    /// <para>This is a client SDK that allows handling application telemetry and lifecycle</para>
    /// </summary>
    public interface ITelimena
    {
        Task<StatisticsUpdateResponse> ReportUsage(string functionName);
        UpdateResponse CheckForUpdates();
        RegistrationResponse RegisterClient();
        Task Initialize();
    }

    internal static class ApiRoutes
    {
        public const string UpdateProgramStatistics = "api/Statistics/UpdateProgramStatistics";
    }
}