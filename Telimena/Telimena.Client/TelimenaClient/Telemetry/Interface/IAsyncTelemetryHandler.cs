using System.Collections.Generic;
using System.Threading.Tasks;

namespace TelimenaClient
{
    /// <summary>
    ///     Asynchronous Telemetry methods
    /// </summary>
    public interface IAsyncTelemetryHandler : IFluentInterface
    {
        /// <summary>
        ///     Initializes the Telimena client.
        ///     <para />
        ///     Each time initialization is called, it will increment the program usage statistics.
        ///     It should be called once per application execution
        ///     <para>This is an ASYNC method which should be awaited</para>
        /// </summary>
        /// <returns></returns>
        Task<TelemetryInitializeResponse> Initialize(Dictionary<string, object> telemetryData = null);

        /// <summary>
        ///     Reports an occurence of an event
        ///     <para>This is an ASYNC method which should be awaited</para>
        /// </summary>
        /// <param name="eventName">Name of the event</param>
        /// <param name="telemetryData">Custom telemetry data</param>
        /// <returns></returns>
        Task Event(string eventName, Dictionary<string, object> telemetryData = null);

        /// <summary>
        ///     Report the usage of the application view.
        ///     <para>This is an ASYNC method which should be awaited</para>
        /// </summary>
        /// <param name="viewName">The name of the view. If left blank, it will report the name of the invoked method</param>
        /// <param name="telemetryData"></param>
        /// <returns></returns>
        Task View(string viewName, Dictionary<string, object> telemetryData = null);
    }

    
}