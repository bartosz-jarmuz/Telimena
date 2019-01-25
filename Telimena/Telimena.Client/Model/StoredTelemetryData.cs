using System.IO;

namespace TelimenaClient
{
    /// <summary>
    /// Class StoredTelemetryData.
    /// </summary>
    internal class StoredTelemetryData
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="StoredTelemetryData" /> class.
        /// </summary>
        /// <param name="fileInfo">The file information.</param>
        public StoredTelemetryData(FileInfo fileInfo)
        {
            File = fileInfo;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StoredTelemetryData" /> class.
        /// </summary>
        public StoredTelemetryData()
        {
        }

        /// <summary>
        /// Gets or sets the file.
        /// </summary>
        /// <value>The file.</value>
        public FileInfo File { get; set; }
        /// <summary>
        /// Gets or sets the serialized data.
        /// </summary>
        /// <value>The serialized data.</value>
        public string SerializedData { get; set; }
    }
}