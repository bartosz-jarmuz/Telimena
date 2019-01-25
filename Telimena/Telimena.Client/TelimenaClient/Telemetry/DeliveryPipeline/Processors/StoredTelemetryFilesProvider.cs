using System.IO;
using System.Threading.Tasks;

namespace TelimenaClient
{
    internal class StoredTelemetryFilesProvider : ITelemetryBroadcastingProcessor
    {

        private readonly DirectoryInfo telemetryDirectory;

        public StoredTelemetryFilesProvider(DirectoryInfo telemetryDirectory)
        {
            this.telemetryDirectory = telemetryDirectory;
        }

        public Task Process(TelemetryBroadcastingContext context)
        {
            foreach (FileInfo fileInfo in this.telemetryDirectory.EnumerateFiles("*.json"))
            {
                context.Data.Add(new StoredTelemetryData(fileInfo));
            }

            return Task.FromResult(true);
        }
    }
}