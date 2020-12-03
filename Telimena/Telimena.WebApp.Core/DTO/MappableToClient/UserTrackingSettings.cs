namespace Telimena.WebApp.Core.DTO.MappableToClient
{
    /// <summary>
    /// The settings of how user information is tracked<br/>
    /// WARNING: In some cases some settings might violate GDPR and other Privacy protection laws,<br/>
    /// so make sure that you have right to collect and process this information,<br/>
    /// and that your storage is secure.
    /// </summary>
    public class UserTrackingSettings
    {
        ///// <summary>
        ///// The user name and machine name will be replaced by reader-friendly random identifiers (well, better than GUID)<br/>
        ///// such as "TackyGorilla","CartographerMarlowe","PeruvianRegan" etc.<br/>
        ///// The identifier will be shared between other apps
        ///// </summary>
        //public static UserTrackingSettings Default
        //{
        //    get
        //    {
        //        return new UserTrackingSettings()
        //        {
        //            UserIdentifierMode = UserIdentifierMode.RandomFriendlyName,
        //            ShareIdentifierWithOtherTelimenaApps = true
        //        };
        //    }
        //}

        ///// <summary>
        ///// Telemetry will not be used
        ///// </summary>
        //public static UserTrackingSettings NoTelemetry
        //{
        //    get
        //    {
        //        return new UserTrackingSettings()
        //        {
        //            UserIdentifierMode = UserIdentifierMode.NoTelemetry,
        //            ShareIdentifierWithOtherTelimenaApps = false
        //        };
        //    }
        //}

        ///// <summary>
        ///// Telemetry will be fully anonymous
        ///// </summary>
        //public static UserTrackingSettings FullyAnonymous
        //{
        //    get
        //    {
        //        return new UserTrackingSettings()
        //        {
        //            UserIdentifierMode = UserIdentifierMode.AnonymousGUID,
        //            ShareIdentifierWithOtherTelimenaApps = false
        //        };
        //    }
        //}

        ///// <summary>
        ///// Telemetry will track personal data<br/>
        ///// WARNING: In some cases some settings might violate GDPR and other Privacy protection laws,<br/>
        ///// so make sure that you have right to collect and process this information,<br/>
        ///// and that your storage is secure.
        ///// </summary>
        //public static UserTrackingSettings TrackPersonalData
        //{
        //    get
        //    {
        //        return new UserTrackingSettings()
        //        {
        //            UserIdentifierMode = UserIdentifierMode.TrackPersonalData,
        //            ShareIdentifierWithOtherTelimenaApps = true
        //        };
        //    }
        //}

        ///// <summary>
        ///// Custom implementation of settings
        ///// </summary>
        ///// <param name="identifierMode"></param>
        ///// <param name="shareIdentifierWithOtherTelimenaApps"></param>
        ///// <returns></returns>
        //public static UserTrackingSettings Custom(UserIdentifierMode identifierMode, bool shareIdentifierWithOtherTelimenaApps)
        //{
        //    return new UserTrackingSettings()
        //    {
        //        UserIdentifierMode = identifierMode,
        //        ShareIdentifierWithOtherTelimenaApps = shareIdentifierWithOtherTelimenaApps
        //    };
        //}

        /// <summary>
        /// Specify what kind of user identifier should be used
        /// </summary>
        public UserIdentifierMode UserIdentifierMode { get; set; }

        /// <summary>
        /// If this is set to true, the same identifier (in particular, the anonymous guid and friendly random name) <br/>
        /// will be used for all applications that use Telimena. <br/>
        /// Otherwise, each application will generate it's own identifier. <br/>
        /// </summary>
        public bool ShareIdentifierWithOtherTelimenaApps { get; set; }
    }
}