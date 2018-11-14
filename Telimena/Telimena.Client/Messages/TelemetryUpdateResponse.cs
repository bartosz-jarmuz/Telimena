namespace TelimenaClient
{
    /// <summary>
    /// Class StatisticsUpdateResponse.
    /// </summary>
    public class TelemetryUpdateResponse : TelimenaResponseBase
    {
        /// <summary>
        /// Gets or sets the name of the component.
        /// </summary>
        /// <value>The name of the component.</value>
        public string ComponentName { get; set; }

        /// <summary>
        /// Gets or sets the component identifier.
        /// </summary>
        /// <value>The component identifier.</value>
        public int ComponentId{ get; set; }
        /// <summary>
        /// Gets or sets the program identifier.
        /// </summary>
        /// <value>The program identifier.</value>
        public int ProgramId { get; set; }
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>The user identifier.</value>
        public int UserId { get; set; }
        /// <summary>
        /// Gets or sets the count.
        /// </summary>
        /// <value>The count.</value>
        public int Count { get; set; }
    }
}