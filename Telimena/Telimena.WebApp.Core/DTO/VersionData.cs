namespace Telimena.WebApp.Core
{
       using System;

        /// <summary>
        /// Holds the version data
        /// </summary>
        public class VersionData
        {
            /// <summary>
            /// Creates new instance
            /// </summary>
            [Obsolete("Only for serialization")]
            public VersionData()
            {

            }

            /// <summary>
            /// Creates new instance
            /// </summary>
            public VersionData(string assemblyVersion, string fileVersion)
            {
                this.AssemblyVersion = assemblyVersion;
                this.FileVersion = fileVersion;
            }

            /// <summary>
            /// Gets or sets the version related to the [assembly: AssemblyVersion("1.0")] attribute
            /// </summary>
            /// <value>The version.</value>
            public string AssemblyVersion { get; set; }

            /// <summary>
            /// The file version which is set by [assembly: AssemblyFileVersion("1.0.0.0")] attribute
            /// <para/><seealso href="https://stackoverflow.com/questions/64602/what-are-differences-between-assemblyversion-assemblyfileversion-and-assemblyin"/> 
            /// </summary>
            public string FileVersion { get; set; }

        }
 }

