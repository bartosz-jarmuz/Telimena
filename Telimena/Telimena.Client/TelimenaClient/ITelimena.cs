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
        /// <summary>
        /// Performs an update check and returns the result which allows custom handling of the update process.
        /// It will return info about beta versions as well.
        /// </summary>
        /// <returns></returns>
        Task<UpdateCheckResult> CheckForUpdates();

        /// <summary>
        /// Handles the updating process from start to end
        /// </summary>
        /// <param name="acceptBeta">Determines whether update packages marked as 'beta' version should be used</param>
        /// <returns></returns>
        Task HandleUpdates(bool acceptBeta);

        /// <summary>
        /// Initializes the Telimena client. <para/>
        /// Each time initialization is called, it will increment the program usage statistics.
        /// It should be called once per application execution
        /// </summary>
        /// <returns></returns>
        Task<RegistrationResponse> Initialize();

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

        /// <summary>
        ///     Report the usage of the application function.
        /// </summary>
        /// <param name="functionName">The name of the function. If left blank, it will report the name of the invoked method</param>
        /// <returns></returns>
        Task<StatisticsUpdateResponse> ReportUsage([CallerMemberName] string functionName = null);

        /// <summary>
        ///     Report the usage of the application function.
        /// </summary>
        /// <param name="customData">A JSON serialized object which contains some program specific custom data</param>
        /// <param name="functionName">The name of the function. If left blank, it will report the name of the invoked method</param>
        /// <returns></returns>
        Task<StatisticsUpdateResponse> ReportUsageWithCustomData(string customData, [CallerMemberName] string functionName = null);

        /// <summary>
        ///     Report the usage of the application function.
        /// </summary>
        /// <param name="customDataObject">A simple data object to be serialized and sent to Telimena. MUST BE JSON SERIALIZABLE</param>
        /// <param name="functionName">The name of the function. If left blank, it will report the name of the invoked method</param>
        /// <returns></returns>
        Task<StatisticsUpdateResponse> ReportUsageWithCustomData<T>(T customDataObject, [CallerMemberName] string functionName = null);

        /// <summary>
        /// If true, then Telimena will swallow any errors. Otherwise, it will rethrow
        /// </summary>
        bool SuppressAllErrors { get; set; }
    }
}