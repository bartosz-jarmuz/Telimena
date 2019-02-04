namespace TelimenaClient.Model
{
    /// <summary>
    /// Enum LogLevel
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// For verbose messages used for diagnosing bugs and issues.
        /// </summary>
        Debug,

        /// <summary>
        /// For general informational messages
        /// </summary>
        Info,

        /// <summary>
        /// For warnings
        /// </summary>
        Warn,

        /// <summary>
        /// For various non-critical error messages.
        /// </summary>
        Error,

        /// <summary>
        /// For disasters and other critical events
        /// </summary>
        Critical,
    }
}