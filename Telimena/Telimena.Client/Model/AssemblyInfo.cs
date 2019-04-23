using System.IO;
using System.Reflection;

namespace TelimenaClient.Model
{
    #region Using

    #endregion

    /// <summary>
    /// Holds data about an assembly
    /// </summary>
    public class AssemblyInfo
    {
        /// <summary>
        /// Creates an instance of AssemblyInfo, where assembly data is read from Assembly through reflection
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        public AssemblyInfo(Assembly assembly)
        {
            AssemblyName assName = assembly.GetName();
            this.Name = assName.Name;
            this.Extension = Path.GetExtension(assembly.CodeBase);

            this.VersionData = new VersionData(
                assemblyVersion : TelimenaVersionReader.Read(assembly, VersionTypes.AssemblyVersion),
                fileVersion: TelimenaVersionReader.Read(assembly, VersionTypes.FileVersion)
            );

            this.Location = assembly.Location;
        }

       
        /// <summary>
        /// Gets or sets the extension.
        /// </summary>
        /// <value>The extension.</value>
        public string Extension { get; set; }

        /// <summary>
        /// New instance of AssemblyInfo
        /// </summary>
        public AssemblyInfo()
        {
        }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>The location.</value>
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }
      
        /// <summary>
        /// Holds the version data
        /// </summary>
        public VersionData VersionData { get; set; }
    }
}