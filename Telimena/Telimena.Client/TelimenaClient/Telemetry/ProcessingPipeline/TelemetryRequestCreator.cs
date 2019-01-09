using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelimenaClient
{
    internal class TelemetryRequestCreator 
    {
        private readonly DirectoryInfo telemetryDirectory;

        public TelemetryRequestCreator(DirectoryInfo telemetryDirectory)
        {
            this.telemetryDirectory = telemetryDirectory;
        }

        public async Task<TelemetryUpdateRequest> Create(Guid telemetryKey, Guid userId, bool debugMode)
        {
            var serializedData = await this.GetAllFilesContent();

            TelemetryUpdateRequest request = new TelemetryUpdateRequest(telemetryKey)
            {
                DebugMode = debugMode, SerializedTelemetryUnits = serializedData.Select(x=>x.Value).ToList(), TelemetryKey = telemetryKey, UserId = userId
            };
            return request;

        }

        private async Task<List<KeyValuePair<FileInfo, string>>> GetAllFilesContent()
        {
            var filesContents = new List<KeyValuePair<FileInfo,string>>();

            foreach (FileInfo fileInfo in this.telemetryDirectory.EnumerateFiles("*.json"))
            {
                StringBuilder stringBuilder = new StringBuilder();

                using (StreamReader sr = fileInfo.OpenText())
                {
                    string line;
                    while ((line = await sr.ReadLineAsync()) != null)
                    {
                        stringBuilder.AppendLine(line);
                    }
                }
                filesContents.Add(new KeyValuePair<FileInfo, string>(fileInfo, stringBuilder.ToString()));
            }

            return filesContents;
        }
    }
}