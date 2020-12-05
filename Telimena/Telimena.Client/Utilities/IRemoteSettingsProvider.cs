using System;
using System.Threading.Tasks;

namespace TelimenaClient
{
    /// <summary>
    /// Provides settings from WEB
    /// </summary>
    public interface IRemoteSettingsProvider
    {
        /// <summary>
        /// Gets user tracking settings
        /// </summary>
        /// <param name="telemetryKey"></param>
        /// <returns></returns>
        Task<string> GetUserTrackingSettings(Guid telemetryKey);
    }
}