namespace Telimena.WebApp.Core.DTO
{
    public class DefaultUpdaterDescriptions
    {
        public static string UpdaterDescription { get; } = "This updater is intended for use with 'standalone' applications. The update package is expected to be a zip archive." +
                                                           " The updater will first close the main application, then extract the content of this package and insert the files into the program directory," +
                                                           " and finally - restart the main application.";

        public static string PackageTriggerUpdaterDescription { get; } = "This updater is intended for use with 'self-installing' packages, e.g. plugins to other software." +
                                                                         " The update package is expected to be able to install itself or present an installation wizard to the user." +
                                                                         " This updater will simply attempt to execute ('run') the update package.";
    }
}