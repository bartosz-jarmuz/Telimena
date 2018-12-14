namespace TelimenaClient
{
    public partial interface IBlockingTelimenaHandler
    {
        /// <summary>
        ///     Performs an update check and returns the result which allows custom handling of the update process.
        ///     It will return info about beta versions as well.
        ///     <para>
        ///         This method is a synchronous wrapper over itsasync counterpart. It will block the thread. It is recommended
        ///         to use async method and handle awaiting properly
        ///     </para>
        /// </summary>
        /// <returns></returns>
        UpdateCheckResult CheckForUpdates();

        /// <summary>
        ///     Handles the updating process from start to end
        ///     <para>
        ///         This method is a synchronous wrapper over itsasync counterpart. It will block the thread. It is recommended
        ///         to use async method and handle awaiting properly
        ///     </para>
        /// </summary>
        /// <param name="acceptBeta">Determines whether update packages marked as 'beta' version should be used</param>
        /// <returns></returns>
        UpdateCheckResult HandleUpdates(bool acceptBeta);
    }
}