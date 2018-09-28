namespace TelimenaClient
{
    /// <summary>
    /// Class RegistrationRequest.
    /// </summary>
    public class RegistrationRequest
    {
        /// <summary>
        /// Gets or sets the user information.
        /// </summary>
        /// <value>The user information.</value>
        public UserInfo UserInfo { get; set; }
        /// <summary>
        /// Gets or sets the program information.
        /// </summary>
        /// <value>The program information.</value>
        public ProgramInfo ProgramInfo { get; set; }
        /// <summary>
        /// Gets or sets the telimena version.
        /// </summary>
        /// <value>The telimena version.</value>
        public string TelimenaVersion { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [skip usage incrementation].
        /// </summary>
        /// <value><c>true</c> if [skip usage incrementation]; otherwise, <c>false</c>.</value>
        public bool SkipUsageIncrementation { get; set; }
    }
}