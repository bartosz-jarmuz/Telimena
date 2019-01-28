using System.Collections.Generic;

namespace TelimenaClient
{
   /// <summary>
   /// Synchronous Telemetry methods
   /// </summary>
    public interface IBlockingTelemetryHandler : IFluentInterface
    {
        /// <summary>
        ///     Initializes the Telimena client.
        ///     <para />
        ///     Each time initialization is called, it will increment the program usage statistics.
        ///     It should be called once per application execution
        ///     <para>
        ///         This method is a synchronous wrapper over its async counterpart. It will block the thread. It is recommended
        ///         to use async method and handle awaiting properly
        ///     </para>
        /// </summary>
        /// <returns></returns>
        TelemetryInitializeResponse Initialize(Dictionary<string, object> telemetryData = null);

        /// <summary>
        ///     Reports an occurence of an event
        ///     <para>
        ///         This method is a synchronous wrapper over its async counterpart. It will block the thread. It is recommended
        ///         to use async method and handle awaiting properly
        ///     </para>
        /// </summary>
        /// <param name="eventName">Name of the event</param>
        /// <param name="telemetryData">Custom telemetry data</param>
        /// <returns></returns>
        TelemetryItem Event(string eventName, Dictionary<string, object> telemetryData = null);

        /// <summary>
        ///     Report the usage of the application view.
        ///     <para>
        ///         This method is a synchronous wrapper over its async counterpart. It will block the thread. It is recommended
        ///         to use async method and handle awaiting properly
        ///     </para>
        /// </summary>
        /// <param name="viewName">The name of the view. If left blank, it will report the name of the invoked method</param>
        /// <param name="telemetryData"></param>
        /// <returns></returns>
        TelemetryItem View(string viewName, Dictionary<string, object> telemetryData = null);
    }
}