using System.Collections.Generic;

namespace Telimena.PackageTriggerUpdater
{
    public class UpdateInstructions
    {
        /// <summary>
        /// Gets or sets the name of the program.
        /// </summary>
        /// <value>The name of the program.</value>
        public string ProgramName { get; set; }
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

            /// <summary>
            /// Gets or sets the release notes.
            /// </summary>
            /// <value>The release notes.</value>
            public string ReleaseNotes { get; set; }
        }
    }
}