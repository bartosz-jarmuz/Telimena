using TelimenaClient.Model;
using TelimenaClient.Model.Internal;

namespace TelimenaClient
{
    /// <summary>
    /// Interface IReceiveUserInput
    /// </summary>
    public interface IReceiveUserInput
    {
        /// <summary>
        /// Shows the install updates now question.
        /// </summary>
        /// <param name="maxVersion"></param>
        /// <param name="totalDownloadSize"></param>
        /// <param name="programInfo"></param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool ShowInstallUpdatesNowQuestion(string maxVersion, long totalDownloadSize, LiveProgramInfo programInfo);

        /// <summary>
        /// Shows the download and install updates question.
        /// </summary>
        /// <param name="maxVersion">The maximum version.</param>
        /// <param name="totalDownloadSize">Total size of the download.</param>
        /// <param name="programInfo">The program information.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool ShowDownloadAndInstallUpdatesQuestion(string maxVersion, long totalDownloadSize, LiveProgramInfo programInfo);
    }
}