namespace TelimenaClient
{
    /// <summary>
    ///     The type of version (.NET has several)
    /// </summary>
    public enum VersionTypes
    {
        /// <summary>
        ///     Gets or sets the version from the [assembly: AssemblyVersion("1.0")] attribute
        /// </summary>
        AssemblyVersion

        , /// <summary>
        ///     The file version which is set by [assembly: AssemblyFileVersion("1.0.0.0")] attribute
        /// </summary>
        FileVersion

    }
}