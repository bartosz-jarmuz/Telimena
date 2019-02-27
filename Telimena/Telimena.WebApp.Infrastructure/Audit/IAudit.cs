using System;

namespace MvcAuditLogger
{
    /// <summary>
    /// Interface Audit
    /// </summary>
    public interface IAudit
    {
        /// <summary>
        /// Gets or sets the area of the page that was accessed.
        /// </summary>
        /// <value>The area accessed.</value>
        string AreaAccessed { get; set; }
        /// <summary>
        /// Gets or sets the audit identifier.
        /// </summary>
        /// <value>The audit identifier.</value>
        Guid AuditID { get; set; }
        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>The data.</value>
        string Data { get; set; }
        /// <summary>
        /// Gets or sets the ip address from which the request came
        /// </summary>
        /// <value>The ip address.</value>
        string IPAddress { get; set; }
        /// <summary>
        /// Gets or sets the session identifier.
        /// </summary>
        /// <value>The session identifier.</value>
        string SessionID { get; set; }
        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        /// <value>The timestamp.</value>
        DateTimeOffset Timestamp { get; set; }
        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>The name of the user.</value>
        string UserName { get; set; }
    }
}