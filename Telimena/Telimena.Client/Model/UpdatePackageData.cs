namespace TelimenaClient
{
    /// <summary>
    /// Class UpdatePackageData.
    /// </summary>
    public class UpdatePackageData
    {
        /// <summary>
        /// Gets or sets the stored file path.
        /// </summary>
        /// <value>The stored file path.</value>
        public string StoredFilePath { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        public string FileName { get; set; }
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id { get; set; }
        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>The version.</value>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the file size bytes.
        /// </summary>
        /// <value>The file size bytes.</value>
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is beta.
        /// </summary>
        /// <value><c>true</c> if this instance is beta; otherwise, <c>false</c>.</value>
        public bool IsBeta { get; set; }
    }
}