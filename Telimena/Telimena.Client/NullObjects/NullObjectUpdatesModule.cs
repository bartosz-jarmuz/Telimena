using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TelimenaClient.Model.Internal;

namespace TelimenaClient
{
    internal class NullObjectUpdatesModule : IUpdatesModule
    {
        private UpdateCheckResult Result =>
            new UpdateCheckResult()
            {
                Exception = new TelimenaException($"Update check handled by {nameof(NullObjectTelemetryModule)}")
            };

        public Task<UpdateCheckResult> CheckForUpdatesAsync(bool acceptBeta = true)
        {
            // This is for when initialization of proper telemetry module would fail - we should never throw errors in client code!
            return Task.FromResult(this.Result);
        }

        public Task<UpdateCheckResult> HandleUpdatesAsync(bool acceptBeta)
        {
            // This is for when initialization of proper telemetry module would fail - we should never throw errors in client code!
            return Task.FromResult(this.Result);

        }

        public UpdateCheckResult CheckForUpdates()
        {
            // This is for when initialization of proper telemetry module would fail - we should never throw errors in client code!
            return this.Result;
        }

        public void InstallUpdates(UpdateCheckResult checkResult)
        {
            // This is for when initialization of proper telemetry module would fail - we should never throw errors in client code!
        }

        public Task InstallUpdatesAsync(UpdateCheckResult checkResult)
        {

            // This is for when initialization of proper telemetry module would fail - we should never throw errors in client code!
            return Task.FromResult(this.Result);

        }

        public UpdateCheckResult HandleUpdates(bool acceptBeta)
        {
            // This is for when initialization of proper telemetry module would fail - we should never throw errors in client code!
            return this.Result;
        }
    }
}