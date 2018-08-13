using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Telimena.Client
{
    #region Using

    #endregion

    /// <summary>
    ///     Telemetry and Lifecycle Management Engine App
    ///     <para>This is a client SDK that allows handling application telemetry and lifecycle</para>
    /// </summary>
    public interface ITelimena
    {
        Task<UpdateCheckResult> CheckForUpdates();
        Task<RegistrationResponse> Initialize();
        Task<StatisticsUpdateResponse> ReportUsage([CallerMemberName] string functionName = null);

        /// <summary>
        ///     Loads the referenced helper assemblies, e.g. for the purpose of updating
        /// </summary>
        /// <param name="assemblies"></param>
        void LoadHelperAssemblies(params Assembly[] assemblies);

        /// <summary>
        ///     Loads the referenced helper assemblies, e.g. for the purpose of updating
        /// </summary>
        /// <param name="assemblyNames"></param>
        void LoadHelperAssembliesByName(params string[] assemblyNames);

        Task HandleUpdates(BetaVersionSettings betaVersionSettings);
    }
}