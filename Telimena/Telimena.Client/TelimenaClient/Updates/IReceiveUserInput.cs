using System.Collections.Generic;

namespace TelimenaClient
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
        /// <param name="maxVersion"></param>
        /// <param name="programInfo"></param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool ShowInstallUpdatesNowQuestion(string maxVersion, LiveProgramInfo programInfo);
    }
}