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
        /// Gets or sets the product.
        /// </summary>
        /// <value>The product.</value>
        public string Product { get; set; }
        /// <summary>
        /// Gets or sets the trademark.
        /// </summary>
        /// <value>The trademark.</value>
        public string Trademark { get; set; }
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; set; }
        /// <summary>
        /// Gets or sets the copyright.
        /// </summary>
        /// <value>The copyright.</value>
        public string Copyright { get; set; }
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; set; }
        /// <summary>
        /// Gets or sets the company.
        /// </summary>
        /// <value>The company.</value>
        public string Company { get; set; }
        /// <summary>
        /// Gets or sets the full name.
        /// </summary>
        /// <value>The full name.</value>
        public string FullName { get; set; }
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