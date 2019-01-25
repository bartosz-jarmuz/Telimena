using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelimenaClient
{
    internal class TelemetryRequestDataReader : ITelemetryBroadcastingProcessor
    {
        public async Task Process(TelemetryBroadcastingContext context)
        {
            foreach (var dataItem in context.Data)
            {
                StringBuilder stringBuilder = new StringBuilder();

                using (StreamReader sr = dataItem.File.OpenText())
                {
                    string line;
                    while ((line = await sr.ReadLineAsync().ConfigureAwait(false)) != null)
                    {
                        stringBuilder.AppendLine(line);
                    }
                }

                dataItem.SerializedData = stringBuilder.ToString();
            }
        }

    }
}