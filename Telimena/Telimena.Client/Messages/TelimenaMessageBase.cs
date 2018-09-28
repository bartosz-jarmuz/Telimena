using System;

namespace Telimena.Client
{
    /// <summary>
    /// Class TelimenaResponseBase.
    /// </summary>
    public abstract class TelimenaResponseBase
    {
        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        /// <value>The exception.</value>
        public Exception Exception { get; set; }
    }
}