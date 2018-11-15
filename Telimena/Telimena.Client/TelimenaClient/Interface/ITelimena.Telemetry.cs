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
    public partial interface ITelimena
    {
        /// <summary>
        /// Initializes the Telimena client. <para/>
        /// Each time initialization is called, it will increment the program usage statistics.
        /// It should be called once per application execution
        /// <para>This is an ASYNC method which should be awaited</para>
        /// </summary>
        /// <returns></returns>
        Task<TelemetryInitializeResponse> InitializeAsync(Dictionary<string, string> telemetryData = null);

        /// <summary>
        /// Initializes the Telimena client. <para/>
        /// Each time initialization is called, it will increment the program usage statistics.
        /// It should be called once per application execution
        /// <para>This method is a synchronous wrapper over itsasync counterpart. It will block the thread. It is recommended to use async method and handle awaiting properly</para>
        /// </summary>
        /// <returns></returns>
        TelemetryInitializeResponse InitializeBlocking(Dictionary<string, string> telemetryData = null);

        /// <summary>
        /// Reports an occurence of an event
        /// <para>This is an ASYNC method which should be awaited</para>
        /// </summary>
        /// <param name="eventName">Name of the event</param>
        /// <param name="telemetryData">Custom telemetry data</param>
        /// <returns></returns>
        Task<TelemetryUpdateResponse> ReportEventAsync(string eventName, Dictionary<string, string> telemetryData = null);

        /// <summary>
        /// Reports an occurence of an event
        /// <para>This method is a synchronous wrapper over its async counterpart. It will block the thread. It is recommended to use async method and handle awaiting properly</para>
        /// </summary>
        /// <param name="eventName">Name of the event</param>
        /// <param name="telemetryData">Custom telemetry data</param>
        /// <returns></returns>
        TelemetryUpdateResponse ReportEventBlocking(string eventName, Dictionary<string, string> telemetryData = null);

        /// <summary>
        ///     Report the usage of the application view.
        /// <para>This is an ASYNC method which should be awaited</para>
        /// </summary>
        /// <param name="viewName">The name of the view. If left blank, it will report the name of the invoked method</param>
        /// <param name="telemetryData"></param>
        /// <returns></returns>
        Task<TelemetryUpdateResponse> ReportViewAccessedAsync(string viewName, Dictionary<string, string> telemetryData = null);

        /// <summary>
        ///     Report the usage of the application view.
        /// <para>This method is a synchronous wrapper over its async counterpart. It will block the thread. It is recommended to use async method and handle awaiting properly</para>
        /// </summary>
        /// <param name="viewName">The name of the view. If left blank, it will report the name of the invoked method</param>
        /// <param name="telemetryData"></param>
        /// <returns></returns>
        TelemetryUpdateResponse ReportViewAccessedBlocking(string viewName, Dictionary<string, string> telemetryData = null);
    }
}