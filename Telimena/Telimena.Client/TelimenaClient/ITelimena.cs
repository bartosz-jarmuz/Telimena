using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace TelimenaClient
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
        /// <para>This is an ASYNC method which should be awaited</para>
        /// </summary>
        /// <param name="acceptBeta"></param>
        /// <returns></returns>
        Task<UpdateCheckResult> CheckForUpdatesAsync(bool acceptBeta = true);


        /// <summary>
        /// Performs an update check and returns the result which allows custom handling of the update process.
        /// It will return info about beta versions as well.
        /// <para>This method is a synchronous wrapper over itsasync counterpart. It will block the thread. It is recommended to use async method and handle awaiting properly</para>
        /// </summary>
        /// <returns></returns>
        UpdateCheckResult CheckForUpdatesBlocking();

        /// <summary>
        /// Handles the updating process from start to end
        /// <para>This is an ASYNC method which should be awaited</para>
        /// </summary>
        /// <param name="acceptBeta">Determines whether update packages marked as 'beta' version should be used</param>
        /// <returns></returns>
        Task<UpdateCheckResult> HandleUpdatesAsync(bool acceptBeta);

        /// <summary>
        /// Handles the updating process from start to end
        /// <para>This method is a synchronous wrapper over itsasync counterpart. It will block the thread. It is recommended to use async method and handle awaiting properly</para>
        /// </summary>
        /// <param name="acceptBeta">Determines whether update packages marked as 'beta' version should be used</param>
        /// <returns></returns>
        UpdateCheckResult HandleUpdatesBlocking(bool acceptBeta);

        /// <summary>
        /// Initializes the Telimena client. <para/>
        /// Each time initialization is called, it will increment the program usage statistics.
        /// It should be called once per application execution
        /// <para>This is an ASYNC method which should be awaited</para>
        /// </summary>
        /// <returns></returns>
        Task<RegistrationResponse> InitializeAsync();

        /// <summary>
        /// Initializes the Telimena client. <para/>
        /// Each time initialization is called, it will increment the program usage statistics.
        /// It should be called once per application execution
        /// <para>This method is a synchronous wrapper over itsasync counterpart. It will block the thread. It is recommended to use async method and handle awaiting properly</para>
        /// </summary>
        /// <returns></returns>
        RegistrationResponse InitializeBlocking();

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

        
        Task<StatisticsUpdateResponse> ReportEventAsync(string eventName, Dictionary<string, string> telemetryData = null);

        /// <summary>
        ///     Report the usage of the application view.
        /// <para>This is an ASYNC method which should be awaited</para>
        /// </summary>
        /// <param name="viewName">The name of the view. If left blank, it will report the name of the invoked method</param>
        /// <returns></returns>
        Task<StatisticsUpdateResponse> ReportUsageAsync([CallerMemberName] string viewName = null);

        /// <summary>
        ///     Report the usage of the application view.
        /// <para>This method is a synchronous wrapper over itsasync counterpart. It will block the thread. It is recommended to use async method and handle awaiting properly</para>
        /// </summary>
        /// <param name="viewName">The name of the view. If left blank, it will report the name of the invoked method</param>
        /// <returns></returns>
        StatisticsUpdateResponse ReportUsageBlocking([CallerMemberName] string viewName = null);

        /// <summary>
        ///     Report the usage of the application view.
        /// <para>This is an ASYNC method which should be awaited</para>
        /// </summary>
        /// <param name="customData">A JSON serialized object which contains some program specific custom data</param>
        /// <param name="viewName">The name of the view. If left blank, it will report the name of the invoked method</param>
        /// <returns></returns>
        Task<StatisticsUpdateResponse> ReportUsageWithCustomDataAsync(string customData, [CallerMemberName] string viewName = null);


        /// <summary>
        ///     Report the usage of the application view.
        /// <para>This method is a synchronous wrapper over itsasync counterpart. It will block the thread. It is recommended to use async method and handle awaiting properly</para>
        /// </summary>
        /// <param name="customData">A JSON serialized object which contains some program specific custom data</param>
        /// <param name="viewName">The name of the view. If left blank, it will report the name of the invoked method</param>
        /// <returns></returns>
        StatisticsUpdateResponse ReportUsageWithCustomDataBlocking(string customData, [CallerMemberName] string viewName = null);

        /// <summary>
        ///     Report the usage of the application view.
        /// <para>This is an ASYNC method which should be awaited</para>
        /// </summary>
        /// <param name="customDataObject">A simple data object to be serialized and sent to Telimena. MUST BE JSON SERIALIZABLE</param>
        /// <param name="viewName">The name of the view. If left blank, it will report the name of the invoked method</param>
        /// <returns></returns>
        Task<StatisticsUpdateResponse> ReportUsageWithCustomDataAsync<T>(T customDataObject, [CallerMemberName] string viewName = null);

        /// <summary>
        ///     Report the usage of the application view.
        /// <para>This method is a synchronous wrapper over itsasync counterpart. It will block the thread. It is recommended to use async method and handle awaiting properly</para>
        /// </summary>
        /// <param name="customDataObject">A simple data object to be serialized and sent to Telimena. MUST BE JSON SERIALIZABLE</param>
        /// <param name="viewName">The name of the view. If left blank, it will report the name of the invoked method</param>
        /// <returns></returns>
        StatisticsUpdateResponse ReportUsageWithCustomDataBlocking<T>(T customDataObject, [CallerMemberName] string viewName = null);

        /// <summary>
        /// If true, then Telimena will swallow any errors. Otherwise, it will rethrow
        /// </summary>
        bool SuppressAllErrors { get; set; }
    }
}