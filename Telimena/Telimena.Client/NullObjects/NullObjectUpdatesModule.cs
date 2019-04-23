using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TelimenaClient.Model.Internal;

namespace TelimenaClient
{
    internal class NullObjectUpdatesModule : IUpdatesModule
    {
        private UpdateCheckResult CheckResult =>
            new UpdateCheckResult()
            {
                Exception = new TelimenaException($"Update check handled by {nameof(NullObjectTelemetryModule)}")
            };

        private UpdateInstallationResult InstallationResult =>
            new UpdateInstallationResult()
            {
                Exception = new TelimenaException($"Update installation handled by {nameof(NullObjectTelemetryModule)}")
            };

        public Task<UpdateCheckResult> CheckForUpdatesAsync(bool acceptBeta = true)
        {
            // This is for when initialization of proper telemetry module would fail - we should never throw errors in client code!
            return Task.FromResult(this.CheckResult);
        }

        public Task<UpdateInstallationResult> HandleUpdatesAsync(bool acceptBeta)
        {
            // This is for when initialization of proper telemetry module would fail - we should never throw errors in client code!
            return Task.FromResult(this.InstallationResult);

        }

        public UpdateCheckResult CheckForUpdates()
        {
            // This is for when initialization of proper telemetry module would fail - we should never throw errors in client code!
            return this.CheckResult;
        }

        public UpdateInstallationResult InstallUpdates(UpdateCheckResult checkResult, bool acceptBeta)
        {
            // This is for when initialization of proper telemetry module would fail - we should never throw errors in client code!
            return this.InstallationResult;
        }

        public Task<UpdateInstallationResult> InstallUpdatesAsync(UpdateCheckResult checkResult, bool acceptBeta)
        {

            // This is for when initialization of proper telemetry module would fail - we should never throw errors in client code!
            return Task.FromResult(this.InstallationResult);

        }

        public UpdateInstallationResult HandleUpdates(bool acceptBeta)
        {
            // This is for when initialization of proper telemetry module would fail - we should never throw errors in client code!
            return this.InstallationResult;
        }
    }
}