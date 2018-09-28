using System.Collections.Generic;

namespace Telimena.ToolkitClient
{
    /// <summary>
    /// Interface IReceiveUserInput
    /// </summary>
    public interface IReceiveUserInput
    {
        /// <summary>
        /// Shows the include beta packages question.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool ShowIncludeBetaPackagesQuestion(UpdateResponse response);
        /// <summary>
        /// Shows the install updates now question.
        /// </summary>
        /// <param name="packagesToInstall">The packages to install.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool ShowInstallUpdatesNowQuestion(IEnumerable<UpdatePackageData> packagesToInstall);
    }
}