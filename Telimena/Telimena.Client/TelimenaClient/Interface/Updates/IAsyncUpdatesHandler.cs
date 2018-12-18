using System.Threading.Tasks;

namespace TelimenaClient
{
    /// <summary>
    /// A set of methods for asynchronous app update handling
    /// </summary>
    public interface IAsyncUpdatesHandler : IFluentInterface
    {
        /// <summary>
        ///     Performs an update check and returns the result which allows custom handling of the update process.
        ///     It will return info about beta versions as well.
        ///     <para>This is an ASYNC method which should be awaited</para>
        /// </summary>
        /// <param name="acceptBeta"></param>
        /// <returns></returns>
        Task<UpdateCheckResult> CheckForUpdates(bool acceptBeta = true);

        /// <summary>
        ///     Handles the updating process from start to end
        ///     <para>This is an ASYNC method which should be awaited</para>
        /// </summary>
        /// <param name="acceptBeta">Determines whether update packages marked as 'beta' version should be used</param>
        /// <returns></returns>
        Task<UpdateCheckResult> HandleUpdates(bool acceptBeta);
    }
}