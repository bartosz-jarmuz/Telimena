using System.Collections.Generic;
using System.Threading.Tasks;
using TelimenaClient.Model;

namespace TelimenaClient
{
    /// <summary>
    ///     Telemetry and Lifecycle Management Engine App
    ///     <para>This is a client SDK that allows handling application telemetry and lifecycle</para>
    /// </summary>
    public interface ITelimena : IFluentInterface
    {
        /// <summary>
        /// Application updating related methods
        /// </summary>
        IUpdatesModule Updates { get; }

        /// <summary>
        /// Telemetry related methods
        /// </summary>
        ITelemetryModule Telemetry { get; }
       
        /// <summary>
        /// Telimena Client properties
        /// </summary>
        ITelimenaProperties Properties { get; }

        /// <summary>
        ///     Initializes the Telimena client.
        ///     <para />
        ///     Each time initialization is called, it will increment the program usage statistics.
        ///     It should be called once per application execution
        ///     <para>This is an ASYNC method which should be awaited</para>
        /// </summary>
        /// <returns></returns>
        Task<TelemetryInitializeResponse> Initialize(Dictionary<string, object> telemetryData = null);
    }
}