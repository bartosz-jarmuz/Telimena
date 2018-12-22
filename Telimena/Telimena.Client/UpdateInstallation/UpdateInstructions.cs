using System.Collections.Generic;

namespace TelimenaClient
{
    /// <summary>
    /// Instructions for update
    /// </summary>
    public class UpdateInstructions
    {
        /// <summary>
        /// Program exe location
        /// </summary>
        public string ProgramExecutableLocation { get; set; }

        /// <summary>
        /// Latest version
        /// </summary>
        public string LatestVersion { get; set; }

        /// <summary>
        /// Package version and path
        /// </summary>
        public List<PackageData> Packages { get; set; }

        /// <summary>
        /// Info about a package
        /// </summary>
        public class PackageData
        {
            /// <summary>
            /// Package version
            /// </summary>
            public string Version { get; set; }
            /// <summary>
            /// Path to package
            /// </summary>
            public string Path { get; set; }

        }
    }
}