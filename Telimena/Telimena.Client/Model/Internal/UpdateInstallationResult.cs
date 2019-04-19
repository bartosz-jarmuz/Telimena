namespace TelimenaClient.Model.Internal
{
    /// <summary>
    /// Returned after handling update installation
    /// </summary>
    public class UpdateInstallationResult
    {
        /// <summary>
        /// The result of update check procedure
        /// </summary>
        public UpdateCheckResult CheckResult { get; set; }

        /// <summary>
        /// Gets or sets the exception which occurred during installation
        /// </summary>
        /// <value>The exception.</value>
        public TelimenaException Exception { get; set; }

    }
}