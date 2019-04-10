using System.Threading.Tasks;
using TelimenaClient.Model.Internal;

namespace TelimenaClient
{
    /// <summary>
    /// Combines all updating related functions
    /// </summary>
    public interface IUpdatesModule : IFluentInterface
    {

        /// <summary>
        ///     Performs an update check and returns the result which allows custom handling of the update process.
        ///     ***IT WILL RETURN INFO ABOUT BETA VERSIONS AS WELL***.
        ///     <para>This is an ASYNC method which should be awaited</para>
        /// </summary>
        /// <param name="acceptBeta"></param>
        /// <returns></returns>
        Task<UpdateCheckResult> CheckForUpdatesAsync(bool acceptBeta = true);

        /// <summary>
        ///     Handles the updating process from start to end
        ///     <para>This is an ASYNC method which should be awaited</para>
        /// </summary>
        /// <param name="acceptBeta">Determines whether update packages marked as 'beta' version should be used</param>
        /// <returns></returns>
        Task<UpdateCheckResult> HandleUpdatesAsync(bool acceptBeta);


        /// <summary>
        ///     Performs an update check and returns the result which allows custom handling of the update process.
        ///     ***IT WILL RETURN INFO ABOUT BETA VERSIONS AS WELL***.
        ///     <para>
        ///         This method is a synchronous wrapper over its async counterpart. It will block the thread. It is recommended
        ///         to use async method and handle awaiting properly
        ///     </para>
        /// </summary>
        /// <returns></returns>
        UpdateCheckResult CheckForUpdates();

        /// <summary>
        ///     Installs the updates specified in the input parameter. It will not ask any questions.
        ///     <para>
        ///         This method is a synchronous wrapper over its async counterpart. It will block the thread. It is recommended
        ///         to use async method and handle awaiting properly
        ///     </para>
        /// </summary>
        /// <param name="checkResult">The result contains info about packages to be installed. You can modify it (e.g. exclude beta ones)</param>
        /// <returns></returns>
        void InstallUpdates(UpdateCheckResult checkResult);

        /// <summary>
        ///     Installs the updates specified in the input parameter. It will not ask any questions.
        /// </summary>
        /// <param name="checkResult">The result contains info about packages to be installed. You can modify it (e.g. exclude beta ones)</param>
        /// <returns></returns>
        Task InstallUpdatesAsync(UpdateCheckResult checkResult);


        /// <summary>
        ///     Handles the updating process from start to end
        ///     <para>
        ///         This method is a synchronous wrapper over its async counterpart. It will block the thread. It is recommended
        ///         to use async method and handle awaiting properly
        ///     </para>
        /// </summary>
        /// <param name="acceptBeta">Determines whether update packages marked as 'beta' version should be used</param>
        /// <returns></returns>
        UpdateCheckResult HandleUpdates(bool acceptBeta);
    }
}