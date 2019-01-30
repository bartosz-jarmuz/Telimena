using System;
using System.Collections.Generic;
using Telimena.WebApp.Core.DTO.MappableToClient;

namespace Telimena.WebApp.Core.DTO
{
    /// <summary>
    /// A single telemetry record of any type
    /// </summary>
    public class TelemetryItem
    {
     

        /// <summary>
        /// 
        /// </summary>
        public TelemetryItem()
        {
        }


        /// <summary>
        /// Type of telemetry item
        /// </summary>
        public TelemetryItemTypes TelemetryItemType { get; set; }

        /// <summary>
        /// FROM APP INSIGHTS
        /// Gets or sets the value that defines absolute order of the telemetry item.
        /// </summary>
        /// <remarks>
        /// The sequence is used to track absolute order of uploaded telemetry items. It is a two-part value that includes 
        /// a stable identifier for the current boot session and an incrementing identifier for each event added to the upload queue:
        /// For UTC this would increment for all events across the system.
        /// For Persistence this would increment for all events emitted from the hosting process.    
        /// The Sequence helps track how many events were fired and how many events were uploaded and enables identification 
        /// of data lost during upload and de-duplication of events on the ingress server.
        /// From <a href="https://microsoft.sharepoint.com/teams/CommonSchema/Shared%20Documents/Schema%20Specs/Common%20Schema%202%20-%20Language%20Specification.docx"/>.
        /// </remarks>
        public string Sequence { get; set; }
        
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>The user identifier.</value>
        public string UserId { get; set; }

        /// <summary>
        /// When an event occurred
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// The name of the telemetry object, such as event (e.g. 'UserLoggedIn') or view ('AdvancedSettingsPanel')
        /// </summary>
        public string EntryKey { get; set; }

        /// <summary>
        /// Holds info about the version components (file, assembly)
        /// </summary>
        public VersionData VersionData { get; set; }

        /// <summary>
        /// Custom telemetry data
        /// </summary>
        public Dictionary<string, string> TelemetryData { get; set; }
    }
}
