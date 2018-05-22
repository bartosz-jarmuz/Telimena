namespace Telimena.Client
{
    #region Using
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    #endregion

    /// <summary>
    ///     Telemetry and Lifecycle Management Engine App
    ///     <para>This is a client SDK that allows handling application telemetry and lifecycle</para>
    /// </summary>
    public interface ITelimena
    {
        Task<UpdateResponse> CheckForUpdates();
        Task<RegistrationResponse> Initialize();
        Task<StatisticsUpdateResponse> ReportUsage([CallerMemberName] string functionName = null);
    }
}