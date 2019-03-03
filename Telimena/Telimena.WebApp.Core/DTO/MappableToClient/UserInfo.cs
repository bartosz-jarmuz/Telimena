namespace Telimena.WebApp.Core.DTO.MappableToClient
{
    /// <summary>
    /// Class UserInfo.
    /// </summary>
    public class UserInfo
    {

        /// <summary>
        /// Gets or sets the name (or other identifier) of the user.
        /// </summary>
        /// <value>The name of the user.</value>
        public string UserIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the authenticated user identifier - if the user is being authenticated in any way
        /// </summary>
        /// <value>The authenticated user identifier.</value>
        public string AuthenticatedUserIdentifier { get; set; }
        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>The email.</value>
        public string Email { get; set; }
        /// <summary>
        /// Gets or sets the name of the machine.
        /// </summary>
        /// <value>The name of the machine.</value>
        public string MachineName { get; set; }
       
    }
}