using System;
using System.Collections.Generic;

namespace TelimenaClient
{
    /// <summary>
    /// Class UpdateResponse.
    /// </summary>
    public class UpdateResponse 
    {
        /// <summary>
        /// Gets or sets the update packages.
        /// </summary>
        /// <value>The update packages.</value>
        public IReadOnlyList<UpdatePackageData> UpdatePackages { get; set; } = new List<UpdatePackageData>();

        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        /// <value>The exception.</value>
        public Exception Exception { get; set; }
    }
}