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
        private class BlockingTelimenaHandler : IBlockingTelimenaHandler
        {
            /// <summary>
            ///     New instance
            /// </summary>
            /// <param name="telimena"></param>
            public BlockingTelimenaHandler(Telimena telimena)
            {
                this.telimena = telimena;
            }

            private readonly Telimena telimena;

            /// <inheritdoc />
            public UpdateCheckResult CheckForUpdates()
            {
                return Task.Run(() => this.telimena.Async.CheckForUpdates()).GetAwaiter().GetResult();
            }

            /// <inheritdoc />
            public UpdateCheckResult HandleUpdates(bool acceptBeta)
            {
                return Task.Run(() => this.telimena.Async.HandleUpdates(acceptBeta)).GetAwaiter().GetResult();
            }

            /// <inheritdoc />
            public TelemetryUpdateResponse ReportViewAccessed(string viewName, Dictionary<string, string> telemetryData = null)
            {
                return Task.Run(() => this.telimena.Async.ReportViewAccessed(viewName, telemetryData)).GetAwaiter().GetResult();
            }

            /// <inheritdoc />
            public TelemetryUpdateResponse ReportEvent(string eventName, Dictionary<string, string> telemetryData = null)
            {
                return Task.Run(() => this.telimena.Async.ReportEvent(eventName, telemetryData)).GetAwaiter().GetResult();
            }

            /// <inheritdoc />
            public TelemetryInitializeResponse Initialize(Dictionary<string, string> telemetryData = null)
            {
                return Task.Run(() => this.telimena.Async.Initialize(telemetryData)).GetAwaiter().GetResult();
            }
        }
    }
}