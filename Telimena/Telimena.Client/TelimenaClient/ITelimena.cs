using System.Collections.Generic;
using System.Threading.Tasks;
using TelimenaClient.Model;

namespace TelimenaClient
{
    /// <summary>
    ///     Tracking and Lifecycle Management Engine App
    ///     <para>This is a client SDK that allows handling application telemetry and lifecycle</para>
    /// </summary>
    public interface ITelimena : IFluentInterface
    {
        /// <summary>
        /// Application updating related methods
        /// </summary>
        IUpdatesModule Updates { get; }

        /// <summary>
        /// Tracking related methods
        /// </summary>
        ITelemetryModule Tracking { get; }
       
        /// <summary>
        /// Telimena Client properties
        /// </summary>
        ITelimenaProperties Properties { get; }

    }
}