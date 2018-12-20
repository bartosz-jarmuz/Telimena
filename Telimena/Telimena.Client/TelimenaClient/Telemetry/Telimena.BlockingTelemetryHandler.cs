using System.Collections.Generic;
using System.Threading.Tasks;

namespace TelimenaClient
{
    #region Using

    #endregion

    public partial class Telimena
    {
        /// <summary>
        ///     Synchronous wrapper around async methods. Uses Task.Run()
        /// </summary>
        internal class BlockingTelemetryHandler : IBlockingTelemetryHandler
        {
            /// <summary>
            ///     New instance
            /// </summary>
            /// <param name="telimena"></param>
            public BlockingTelemetryHandler(Telimena telimena)
            {
                this.telimena = telimena;
            }

            private readonly ITelimena telimena;

            /// <inheritdoc />
            public TelemetryUpdateResponse View(string viewName, Dictionary<string, object> telemetryData = null)
            {
                return Task.Run(() => this.telimena.Telemetry.Async.View(viewName, telemetryData)).GetAwaiter().GetResult();
            }

            /// <inheritdoc />
            public TelemetryUpdateResponse Event(string eventName, Dictionary<string, object> telemetryData = null)
            {
                return Task.Run(() => this.telimena.Telemetry.Async.Event(eventName, telemetryData)).GetAwaiter().GetResult();
            }

            /// <inheritdoc />
            public TelemetryInitializeResponse Initialize(Dictionary<string, object> telemetryData = null)
            {
                return Task.Run(() => this.telimena.Telemetry.Async.Initialize(telemetryData)).GetAwaiter().GetResult();
            }
        }
    }
}