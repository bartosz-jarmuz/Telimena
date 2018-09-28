namespace Telimena.Client
{
    /// <summary>
    /// Class StatisticsUpdateRequest.
    /// </summary>
    public class StatisticsUpdateRequest
    {
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>The user identifier.</value>
        public int UserId { get; set; }
        /// <summary>
        /// Gets or sets the program identifier.
        /// </summary>
        /// <value>The program identifier.</value>
        public int ProgramId { get; set; }
        /// <summary>
        /// Gets or sets the name of the function.
        /// </summary>
        /// <value>The name of the function.</value>
        public string FunctionName { get; set; }
        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>The version.</value>
        public string Version { get; set; }
        /// <summary>
        /// Gets or sets the custom data.
        /// </summary>
        /// <value>The custom data.</value>
        public string CustomData { get; set; }
    }
}