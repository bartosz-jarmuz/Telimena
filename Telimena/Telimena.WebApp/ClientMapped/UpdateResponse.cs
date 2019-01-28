using System.Collections.Generic;

namespace TelimenaClient
{
    /// <summary>
    /// Class UpdateResponse.
    /// </summary>
    public class UpdateResponse : TelimenaResponseBase
    {
        /// <summary>
        /// Gets or sets the update packages.
        /// </summary>
        /// <value>The update packages.</value>
        public IReadOnlyList<UpdatePackageData> UpdatePackages { get; set; } = new List<UpdatePackageData>();
    }
}