using System.Collections.Generic;

namespace TelimenaClient
{
    /// <summary>
    /// Class StatisticsUpdateRequest.
    /// </summary>
    public class StatisticsUpdateRequest
    {
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>The user identifier.</value>
        public int UserId { get; set; }
        /// <summary>
        /// Gets or sets the program identifier.
        /// </summary>
        /// <value>The program identifier.</value>
        public int ProgramId { get; set; }
        /// <summary>
        /// Gets or sets the name of the view.
        /// </summary>
        /// <value>The name of the view.</value>
        public string ComponentName { get; set; }
        
        /// <summary>
        /// Gets or sets the version.
        /// <para/><seealso href="https://stackoverflow.com/questions/64602/what-are-differences-between-assemblyversion-assemblyfileversion-and-assemblyin"/> 
        /// </summary>
        /// <value>The version.</value>
        public string AssemblyVersion { get; set; }

        /// <summary>
        /// The file version which is set by [assembly: AssemblyFileVersion("1.0.0.0")] attribute
        /// <para/><seealso href="https://stackoverflow.com/questions/64602/what-are-differences-between-assemblyversion-assemblyfileversion-and-assemblyin"/> 
        /// </summary>
        public string FileVersion { get; set; }

        /// <summary>
        /// Gets or sets the custom data.
        /// </summary>
        /// <value>The custom data.</value>
        public Dictionary<string, string> TelemetryData { get; set; }
    }
}