namespace Telimena.WebApp.Core.DTO.MappableToClient
{
    #region Using

    #endregion

    /// <summary>
    /// Holds data about an assembly
    /// </summary>
    public class AssemblyInfo
    {
       
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