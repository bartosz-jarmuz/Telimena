using System.IO;

namespace TelimenaClient.Model.Internal
{
    /// <summary>
    /// Holds the result of file download
    /// </summary>
    public class FileDownloadResult
    {
        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the stream.
        /// </summary>
        /// <value>The stream.</value>
        public Stream Stream { get; set; }
    }
}
