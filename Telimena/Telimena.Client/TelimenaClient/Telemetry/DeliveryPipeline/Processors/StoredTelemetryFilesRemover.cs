using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TelimenaClient
{
    internal class StoredTelemetryFilesRemover :  ITelemetryBroadcastingProcessor
    {
        public async Task Process(TelemetryBroadcastingContext context)
        {
            foreach (var data in context.Data)
            {
                try
                {
                    await Retrier.RetryAsync(() => { File.Delete(data.File.FullName); }
                        , TimeSpan.FromMilliseconds(250), 3).ConfigureAwait(false);
                }
                catch
                {
                    //if it fails, it fails
                    //it will be processed next time, and hopefully removed
                    //the API will not allow the same items to be added anyway
                }
            }
        }
    }
}