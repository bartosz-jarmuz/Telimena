using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace TelimenaClient
{
    #region Using

    #endregion

    /// <summary>
    ///     Telemetry and Lifecycle Management Engine App
    ///     <para>This is a client SDK that allows handling application telemetry and lifecycle</para>
    /// </summary>
    public partial interface ITelimena
    {
        /// <summary>
        /// Performs an update check and returns the result which allows custom handling of the update process.
        /// It will return info about beta versions as well.
        /// <para>This is an ASYNC method which should be awaited</para>
        /// </summary>
        /// <param name="acceptBeta"></param>
        /// <returns></returns>
        Task<UpdateCheckResult> CheckForUpdatesAsync(bool acceptBeta = true);

        /// <summary>
        /// Performs an update check and returns the result which allows custom handling of the update process.
        /// It will return info about beta versions as well.
        /// <para>This method is a synchronous wrapper over itsasync counterpart. It will block the thread. It is recommended to use async method and handle awaiting properly</para>
        /// </summary>
        /// <returns></returns>
        UpdateCheckResult CheckForUpdatesBlocking();

        /// <summary>
        /// Handles the updating process from start to end
        /// <para>This is an ASYNC method which should be awaited</para>
        /// </summary>
        /// <param name="acceptBeta">Determines whether update packages marked as 'beta' version should be used</param>
        /// <returns></returns>
        Task<UpdateCheckResult> HandleUpdatesAsync(bool acceptBeta);

        /// <summary>
        /// Handles the updating process from start to end
        /// <para>This method is a synchronous wrapper over itsasync counterpart. It will block the thread. It is recommended to use async method and handle awaiting properly</para>
        /// </summary>
        /// <param name="acceptBeta">Determines whether update packages marked as 'beta' version should be used</param>
        /// <returns></returns>
        UpdateCheckResult HandleUpdatesBlocking(bool acceptBeta);

    }
}