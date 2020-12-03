namespace TelimenaClient.Model
{
    /// <summary>
    /// The settings of how user information is tracked<br/>
    /// WARNING: In some cases some settings might violate GDPR and other Privacy protection laws,<br/>
    /// so make sure that you have right to collect and process this information,<br/>
    /// and that your storage is secure.
    /// </summary>
    public enum UserIdentifierMode

    {
        /// <summary>
        /// The user name and machine name will be replaced by reader-friendly random identifiers (well, better than GUID)<br/>
        /// such as "TackyGorilla","CartographerMarlowe","PeruvianRegan" etc.
        /// </summary>
        RandomFriendlyName,
        /// <summary>
        /// Only a GUID will be generated and sent to the server
        /// </summary>
        AnonymousGUID,
        /// <summary>
        /// Telemetry is not used at all, no user data will be sent over
        /// </summary>
        NoTelemetry,
        /// <summary>
        /// User personal data (Environment.UserName and Environment.MachineName) will be sent over<br/>
        /// WARNING: This might violate GDPR and other Privacy protection laws,<br/>
        /// so make sure that you have right to collect and process this information,<br/>
        /// and that your storage is secure.
        /// </summary>
        TrackPersonalData,
    }
}