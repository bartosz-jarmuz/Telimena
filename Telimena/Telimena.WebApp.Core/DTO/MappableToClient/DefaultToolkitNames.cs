namespace Telimena.WebApp.Core.DTO.MappableToClient
{
    /// <summary>
    /// Class DefaultToolkitNames.
    /// </summary>
    public static class DefaultToolkitNames
    {

        /// <summary>
        /// The updater file name
        /// </summary>
        public static string UpdaterFileName {get;} =  "Updater.exe";

        /// <summary>
        /// The file name of the package trigger updater
        /// </summary>
        public static string PackageTriggerUpdaterFileName {get;} =  "PackageTriggerUpdater.exe";
        /// <summary>
        /// The zipped package file name
        /// </summary>
        public static string ZippedPackageName {get;} =  "Telimena.Client.zip";
        /// <summary>
        /// The telimena assembly name
        /// </summary>
        public static string TelimenaAssemblyName {get;} =  "Telimena.Client.dll";

        /// <summary>
        /// The default updater internal name
        /// </summary>
        public static string UpdaterInternalName {get;} =  "TelimenaStandaloneUpdater";

        public static string UpdaterDescription {get;} =  "This updater is intended for use with 'standalone' applications. The update package is expected to be a zip archive." +
                                                          " The updater will first close the main application, then extract the content of this package and insert the files into the program directory," +
                                                          " and finally - restart the main application.";

        /// <summary>
        /// The default package trigger updater name
        /// </summary>
        public static string PackageTriggerUpdaterInternalName {get;} =  "TelimenaPackageUpdater";

        public static string PackageTriggerUpdaterDescription {get;} =  "This updater is intended for use with 'self-installing' packages, e.g. plugins to other software." +
                                                                        " The update package is expected to be able to install itself or present an installation wizard to the user." +
                                                                        " This updater will simply attempt to execute ('run') the update package.";

        /// <summary>
        /// The telimena system dev team
        /// </summary>
        public static string TelimenaSystemDevTeam {get;} =  "TelimenaSystemDevTeam";

        /// <summary>
        /// Gets the telimena custom exception note key (added to telemetry item)
        /// </summary>
        /// <value>The telimena custom exception note.</value>
        public static string TelimenaCustomExceptionNoteKey { get; } = "TelimenaCustomExceptionNote";

        /// <summary>
        /// Gets or sets the exception unhandled by user code key (added to telemetry item).
        /// </summary>
        /// <value>The exception unhandled by user code key.</value>
        public static string ExceptionUnhandledByUserCodeKey { get; } = "ExceptionUnhandledByUserCode";
    }
}