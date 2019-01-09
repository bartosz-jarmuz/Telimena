using System;
using System.IO;
using System.Threading.Tasks;

namespace TelimenaClient
{
    internal class PersistingPipelineProcessor : ITelemetryPipelineProcessor
    {
        private readonly DirectoryInfo telemetryDirectory;

        public PersistingPipelineProcessor(DirectoryInfo telemetryDirectory)
        {
            this.telemetryDirectory = telemetryDirectory;
        }

        private string GetTelemetryFilePath(TelemetryPipelineTransportUnit unit)
        {
            return Path.Combine(this.telemetryDirectory.FullName, unit.TelemetryItem.Id + ".json");
        }

        public async Task<TelemetryPipelineTransportUnit> Process(TelemetryPipelineTransportUnit unit)
        {
            if (unit.SerializedEntry != null)
            {
                try
                {

                    await Retrier.RetryAsync(() => File.WriteAllText(this.GetTelemetryFilePath(unit), unit.SerializedEntry)
                        , retryInterval: TimeSpan.FromMilliseconds(100));
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error while storing telemetry file: [{this.GetTelemetryFilePath(unit)}]. Data [{unit.SerializedEntry}]", ex);
                }
            }

            return unit;
        }
    }
}