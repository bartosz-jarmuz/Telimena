namespace TelimenaClient
{
    /// <summary>
    /// Class StatisticsUpdateResponse.
    /// </summary>
    public class StatisticsUpdateResponse : TelimenaResponseBase
    {
        /// <summary>
        /// Gets or sets the name of the function.
        /// </summary>
        /// <value>The name of the function.</value>
        public string FunctionName { get; set; }

        /// <summary>
        /// Gets or sets the function identifier.
        /// </summary>
        /// <value>The function identifier.</value>
        public int FunctionId{ get; set; }
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