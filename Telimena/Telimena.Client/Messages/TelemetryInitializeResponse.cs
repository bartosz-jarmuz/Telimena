namespace TelimenaClient
{
    /// <summary>
    /// Class TelemetryInitializeResponse.
    /// </summary>
    public class TelemetryInitializeResponse : TelimenaResponseBase
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
        /// Gets or sets the count.
        /// </summary>
        /// <value>The count.</value>
        public int Count { get; set; }
    }
}