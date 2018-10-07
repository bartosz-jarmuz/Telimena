using System;

namespace MvcAuditLogger
{
    #region Using

    #endregion

    public class Audit : IAudit
    {
        /// <summary>
        ///     Gets or sets the area accessed.
        /// </summary>
        /// <value>The area accessed.</value>
        public string AreaAccessed { get; set; }
        /// <summary>
        ///     Gets or sets the audit identifier.
        /// </summary>
        /// <value>The audit identifier.</value>
        public Guid AuditID { get; set; }
        /// <summary>
        ///     Gets or sets the data.
        /// </summary>
        /// <value>The data.</value>
        public string Data { get; set; }
        /// <summary>
        ///     Gets or sets the ip address.
        /// </summary>
        /// <value>The ip address.</value>
        public string IPAddress { get; set; }
        /// <summary>
        ///     Gets or sets the session identifier.
        /// </summary>
        /// <value>The session identifier.</value>
        public string SessionID { get; set; }
        /// <summary>
        ///     Gets or sets the timestamp.
        /// </summary>
        /// <value>The timestamp.</value>
        public DateTime Timestamp { get; set; }
        /// <summary>
        ///     Gets or sets the name of the user.
        /// </summary>
        /// <value>The name of the user.</value>
        public string UserName { get; set; }
    }
}